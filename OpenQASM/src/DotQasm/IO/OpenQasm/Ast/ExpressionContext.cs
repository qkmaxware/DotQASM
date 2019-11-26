using System;

namespace DotQasm.IO.OpenQasm.Ast {

public enum ArithmeticOperation {
    Addition, Subtraction, Multiplication, Division, Power
}

public interface IExpressionContext {

}

public class ExpressionLiteralContext: IExpressionContext {
    public double Literal {get; private set;}
    public string VariableName {get; private set;}

    public bool IsVariable => VariableName != null;

    public ExpressionLiteralContext(double value) {
        this.Literal = value;
        this.VariableName = null;
    }

    public ExpressionLiteralContext(string name) {
        this.Literal = default;
        this.VariableName = name;
    }
}

public class FunctionCallExpressionContext: IExpressionContext {
    public Func<double, double> Function {get; private set;}
    public IExpressionContext Evaluatable {get; private set;}

    public FunctionCallExpressionContext(Func<double, double> fn, IExpressionContext evaluate) {
        this.Function = fn;
        this.Evaluatable = evaluate;
    }
}

public class ArithmeticExpressionContext: IExpressionContext {

    public IExpressionContext LHS {get; private set;}
    public IExpressionContext RHS {get; private set;}
    public ArithmeticOperation Operation {get; private set;}

    public ArithmeticExpressionContext(IExpressionContext lhs, ArithmeticOperation operation, IExpressionContext rhs) {
       this.LHS = lhs;
       this.Operation = operation;
       this.RHS = rhs; 
    }

}

}