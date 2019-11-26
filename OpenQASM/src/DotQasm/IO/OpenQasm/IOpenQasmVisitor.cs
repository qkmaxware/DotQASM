using DotQasm.IO.OpenQasm.Ast;

namespace DotQasm.IO.OpenQasm {

/// <summary>
/// Interface representing an object that can perform operations on specific OpenQASM AST nodes
/// </summary>
public interface IOpenQasmVisitor {

    /// <summary>
    /// Visit a program
    /// </summary>
    /// <param name="program">program to visit</param>
    void VisitProgram (ProgramContext program);

    /// <summary>
    /// Visit a variable declaration
    /// </summary>
    /// <param name="program">declaration to visit</param>
    void VisitDeclaration (DeclContext declaration);

    /// <summary>
    /// Visit a gate declaration 
    /// </summary>
    /// <param name="program">declaration to visit</param>
    void VisitGateDeclaration (GateDeclContext declaration);

    /// <summary>
    /// Visit an opaque gate declaration
    /// </summary>
    /// <param name="program">declaration to visit</param>
    void VisitOpaqueGateDeclaration (OpaqueGateDeclContext declaration);

    /// <summary>
    /// Visit a classical if
    /// </summary>
    /// <param name="program">conditional to visit</param>
    void VisitClassicalIf (IfContext @if);

    /// <summary>
    /// Visit a barrier
    /// </summary>
    /// <param name="program">barrier to visit</param>
    void VisitBarrier (BarrierContext barrier);

    /// <summary>
    /// Visit a quantum operator application
    /// </summary>
    /// <param name="program">statement to visit</param>
    void VisitUnitaryQuantumOperator (UnitaryOperationContext qop);

    /// <summary>
    /// Visit a measurement
    /// </summary>
    /// <param name="measure">measurement to visit</param>
    void VisitMeasurement (MeasurementContext measure);

    /// <summary>
    /// Visit a reset operation
    /// </summary>
    /// <param name="measure">reset operation to visit</param>
    void VisitReset (ResetContext measure);

}

}