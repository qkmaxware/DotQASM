<p align="center">
  <img width="120" height="120" src="docs/logo.svg">
</p>

Source code and experimental data for MSc. Computer Science thesis. 

## License
See [License](LICENSE.md) for license details.

## Technology
1. Dotnet Standard 2.1+
2. C# 8+

# To Run Experiments
1. Modify experiment parameters in `OpenQASM.Experiment\experiments\Experiment1.cs` lines 27 to 35.
   1. [Optional] Change the experiment group on line 22.
2. Call `dotnet run --project OpenQASM.Experiment` to generate data files
3. Repeat steps 1 and 2 as much as necessary.
4. Call `dotnet run --project OpenQASM.Experiment.Summary` and select the appropriate experiment group to summarize. This produces sums, means, and standard deviation files for each group. 