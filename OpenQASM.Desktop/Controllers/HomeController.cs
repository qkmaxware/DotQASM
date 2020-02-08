using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DotQASM.Desktop.Models;
using DotQasm.IO;
using DotQasm.Backend;
using System.Text.Json;

namespace DotQASM.Desktop.Controllers {

    public class HomeController : Controller {
        
        private static ServerConfigModel settings = new ServerConfigModel();
        private static VirtualDirectory buildContext;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) {
            _logger = logger;
            LoadSettings();

            HomeController.buildContext = new VirtualDirectory(".");
            var qelib1_inc = new VirtualFile(
                "qelib1.inc", 
@"// Quantum Experience (QE) Standard Header
// file: qelib1.inc

// --- QE Hardware primitives ---

// 3-parameter 2-pulse single qubit gate
gate u3(theta,phi,lambda) q { U(theta,phi,lambda) q; }
// 2-parameter 1-pulse single qubit gate
gate u2(phi,lambda) q { U(pi/2,phi,lambda) q; }
// 1-parameter 0-pulse single qubit gate
gate u1(lambda) q { U(0,0,lambda) q; }
// controlled-NOT
gate cx c,t { CX c,t; }
// idle gate (identity)
gate id a { U(0,0,0) a; }

// --- QE Standard Gates ---

// Pauli gate: bit-flip
gate x a { u3(pi,0,pi) a; }
// Pauli gate: bit and phase flip
gate y a { u3(pi,pi/2,pi/2) a; }
// Pauli gate: phase flip
gate z a { u1(pi) a; }
// Clifford gate: Hadamard
gate h a { u2(0,pi) a; }
// Clifford gate: sqrt(Z) phase gate
gate s a { u1(pi/2) a; }
// Clifford gate: conjugate of sqrt(Z)
gate sdg a { u1(-pi/2) a; }
// C3 gate: sqrt(S) phase gate
gate t a { u1(pi/4) a; }
// C3 gate: conjugate of sqrt(S)
gate tdg a { u1(-pi/4) a; }

// --- Standard rotations ---
// Rotation around X-axis
gate rx(theta) a { u3(theta,-pi/2,pi/2) a; }
// rotation around Y-axis
gate ry(theta) a { u3(theta,0,0) a; }
// rotation around Z axis
gate rz(phi) a { u1(phi) a; }

// --- QE Standard User-Defined Gates  ---

// controlled-Phase
gate cz a,b { h b; cx a,b; h b; }
// controlled-Y
gate cy a,b { sdg b; cx a,b; s b; }
// controlled-H
gate ch a,b {
h b; sdg b;
cx a,b;
h b; t b;
cx a,b;
t b; h b; s b; x b; s a;
}
// C3 gate: Toffoli
gate ccx a,b,c
{
  h c;
  cx b,c; tdg c;
  cx a,c; t c;
  cx b,c; tdg c;
  cx a,c; t b; t c; h c;
  cx a,b; t a; tdg b;
  cx a,b;
}
// controlled rz rotation
gate crz(lambda) a,b
{
  u1(lambda/2) b;
  cx a,b;
  u1(-lambda/2) b;
  cx a,b;
}
// controlled phase rotation
gate cu1(lambda) a,b
{
  u1(lambda/2) a;
  cx a,b;
  u1(-lambda/2) b;
  cx a,b;
  u1(lambda/2) b;
}
// controlled-U
gate cu3(theta,phi,lambda) c, t
{
  // implements controlled-U(theta,phi,lambda) with  target t and control c
  u1((lambda-phi)/2) t;
  cx c,t;
  u3(-theta/2,0,-(phi+lambda)/2) t;
  cx c,t;
  u3(theta/2,phi,0) t;
}");
            HomeController.buildContext.AddFile(qelib1_inc);
        }

        private void LoadSettings() {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Name
            );

            var settings = Path.Combine(path, "appconfig.json");

            if (System.IO.File.Exists(settings)) {
                HomeController.settings = JsonSerializer.Deserialize<ServerConfigModel>(System.IO.File.ReadAllText(settings));
                _logger.LogInformation(string.Format("App config loaded '{0}'", settings));
            }   
        }

        private void SaveSettings() {
            var json = JsonSerializer.Serialize<ServerConfigModel>(HomeController.settings);

            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Name
            );

            var settings = Path.Combine(path, "appconfig.json");

            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            using (StreamWriter writer = new StreamWriter(settings)) {
                writer.Write(json);
            }
             _logger.LogInformation(string.Format("App config updated '{0}'", settings));
        }

        public IActionResult Index() {
            var appconfig = new ApplicationConfigModel();
            appconfig.Providers = DotQasm.Tools.Commands.Run.Providers.Select(
                provider => {
                    var model = new ProviderModel();
                    model.Name = provider.ProviderName;
                    model.Backends = provider.ListBackends().Select(info => info.Name).ToArray();
                    return model;
                }
            ).ToArray();
            return View(appconfig);
        }

        public IActionResult Settings() {
            return View(settings);
        }

        public IActionResult UpdateSettings(ServerConfigModel settings) {
            HomeController.settings = settings;
            
            SaveSettings();

            return Json(new {
                success = true,
            });
        }

        [HttpPost]
        public IActionResult SubmitQasm(SubmissionModel model) {
            var circuits = model.qasms.Select(qasm => DotQasm.IO.OpenQasm.Parser.ParseCircuit(qasm, buildContext)).ToList();
            var qubits = circuits.Select(circ => circ.QubitCount).Max();

            var backendOrError = DotQasm.Tools.Commands.Run.GetBackend(
                model.provider, 
                model.backend, 
                settings.FindProperty("Providers", model.provider, "Access Token")?.Value ?? string.Empty, 
                qubits > 0 ? qubits : 1
            );
            if (backendOrError is Exception) {
                Console.Error.WriteLine(((Exception)backendOrError).Message);
            }
            var backend = (IBackend)backendOrError;

            var tasks = new List<Task<BackendResult>>(circuits.Count);
            foreach (var circuit in circuits) {
                var task = backend.Exec(circuit);
                task.Start();
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());

            return View("Results", tasks.Select(task => task.Result));
        }

        public IActionResult ValidateQasm (CodeModel code) {
            try {
                var circ = DotQasm.IO.OpenQasm.Parser.ParseCircuit(code.qasm, buildContext);
                return Json(new {
                    success = true,
                    data = code,
                });
            } catch (DotQasm.IO.OpenQasm.OpenQasmException ex) {
                return Json(new {
                    success = false,
                    error = ex.Format("OpenQASM Script", code.qasm),
                    data = code,
                });
            } catch (Exception e) {
                return Json(new {
                    success = false,
                    error = e.ToString(),
                    data = code,
                });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
