using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DotQASM.Desktop.Models;
using DotQasm;
using DotQasm.Backend;

namespace DotQASM.Desktop.Controllers {

    public class HomeController : Controller {
        
        ServerConfigModel settings = new ServerConfigModel();

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) {
            _logger = logger;

            // TODO load settings
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
            this.settings = settings;
            
            // TODO Persist settings

            return null;
        }

        [HttpPost]
        public IActionResult SubmitQasm(SubmissionModel model) {
            var circuits = model.qasms.Select(qasm => DotQasm.IO.OpenQasm.Parser.ParseCircuit(qasm, "BuildContext")).ToList();
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
                var circ = DotQasm.IO.OpenQasm.Parser.ParseCircuit(code.qasm, "BuildContext");
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
