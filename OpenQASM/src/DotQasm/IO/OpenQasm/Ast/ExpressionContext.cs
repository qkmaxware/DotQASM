using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.IO.OpenQasm.Ast {

public enum ArithmeticOperation {
    Addition, Subtraction, Multiplication, Division, Power
}

public interface IExpressionContext {
    double Evaluate(Dictionary<string, double> variables);
    IEnumerable<ExpressionLiteralContext> GetVariables();
}

public class ExpressionLiteralContext: OpenQasmAstContext, IExpressionContext {
    public double Literal {get; private set;}
    public string VariableName {get; private set;}

    public bool IsVariable => VariableName != null;

    public ExpressionLiteralContext(int position, double value): base(position) {
        this.Literal = value;
        this.VariableName = null;
    }

    public ExpressionLiteralContext(int position, string name): base(position) {
        this.Literal = default;
        this.VariableName = name;
    }

    public double Evaluate(Dictionary<string, double> variables) {
        if (IsVariable) {
            if (variables.ContainsKey(VariableName)) {
                return variables[VariableName];
            } else {
                throw new OpenQasmSemanticException(this.Position, string.Format("'{0}' is not defined in the current scope", VariableName));
            }
        } else {
            return Literal;
        }
    }

    public IEnumerable<ExpressionLiteralContext> GetVariables() {
        if (IsVariable) {
            return new ExpressionLiteralContext[]{this};
        } else {
            return new ExpressionLiteralContext[]{};
        }
    }
}

public class FunctionCallExpressionContext: OpenQasmAstContext, IExpressionContext {
    public Func<double, double> Function {get; private set;}
    public IExpressionContext Evaluatable {get; private set;}

    public FunctionCallExpressionContext(int position, Func<double, double> fn, IExpressionContext evaluate): base(position) {
        this.Function = fn;
        this.Evaluatable = evaluate;
    }

    public double Evaluate(Dictionary<string, double> variables) {
        return Function.Invoke(Evaluatable.Evaluate(variables));
    }

    public IEnumerable<ExpressionLiteralContext> GetVariables() {
        return Evaluatable.GetVariables();
    }
}

public class ArithmeticExpressionContext: OpenQasmAstContext, IExpressionContext {

    public IExpressionContext LHS {get; private set;}
    public IExpressionContext RHS {get; private set;}
    public ArithmeticOperation Operation {get; private set;}

    public ArithmeticExpressionContext(int position, IExpressionContext lhs, ArithmeticOperation operation, IExpressionContext rhs): base(position) {
       this.LHS = lhs;
       this.Operation = operation;
       this.RHS = rhs; 
    }

    public double Evaluate(Dictionary<string, double> variables) {
        return Operation switch {
            ArithmeticOperation.Power           => Math.Pow(LHS.Evaluate(variables), RHS.Evaluate(variables)),
            ArithmeticOperation.Addition        => LHS.Evaluate(variables) + RHS.Evaluate(variables),
            ArithmeticOperation.Subtraction     => LHS.Evaluate(variables) - RHS.Evaluate(variables),
            ArithmeticOperation.Division        => LHS.Evaluate(variables) / RHS.Evaluate(variables),
            ArithmeticOperation.Multiplication  => LHS.Evaluate(variables) * RHS.Evaluate(variables),
            _                                   => LHS.Evaluate(variables)
        };
    }

    public IEnumerable<ExpressionLiteralContext> GetVariables() {
        return LHS.GetVariables().Concat(RHS.GetVariables());
    }

}

}