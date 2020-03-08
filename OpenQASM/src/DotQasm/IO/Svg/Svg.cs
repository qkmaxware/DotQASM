using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Colour = System.Drawing.Color;
using System.Collections;

namespace DotQasm.IO.Svg {

/// <summary>
/// Vector graphics shape bounding rectangle
/// </summary>
public class BoundingBox {
    public float MinX {get; private set;}
    public float MinY {get; private set;}
    public float MaxX => MinX + Width;
    public float MaxY => MinY + Height;
    public float Width {get; private set;}
    public float Height {get; private set;}

    public float MidpointX => MinX + (Width / 2.0f);
    public float MidpointY => MinY + (Height / 2.0f);

    public BoundingBox() {
        MinX = 0; MinY = 0; Width = 0; Height = 0;
    }

    public BoundingBox(float x, float y, float w, float h) {
        this.MinX = x;
        this.MinY = y;
        this.Width = w;
        this.Height = h;
    }

    public BoundingBox Combine(BoundingBox other) {
        float x = Math.Min(MinX, other.MinX);
        float y = Math.Min(MinY, other.MinY);
        float ex = Math.Max(MaxX, other.MaxX);
        float ey = Math.Max(MaxY, other.MaxY);
        float w = (ex - x);
        float h = (ey - y);

        return new BoundingBox(x, y, w, h);
    }
}

/// <summary>
/// Generic SVG shape
/// </summary>
public abstract class SvgShape {
    public Colour StrokeColour = Colour.Black;
    public Colour FillColour = Colour.White;
    public uint StrokeWidth = 1;

    public BoundingBox Bounds {get; protected set;}

    protected string ToHex(Colour c) {
        return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }

    public abstract string SvgElement();
}

/// <summary>
/// 2d SVG line
/// </summary>
public class Line : SvgShape {

    public Vector2 Start {get; private set;}
    public Vector2 End {get; private set;}

    public string EndMarker = "none";
    public string StartMarker = "none";

    public Line(Vector2 start, Vector2 end) {
        this.Start = start;
        this.End = end;

        var min = Vector2.Min(start, end);
        var max = Vector2.Max(start, end);

        this.Bounds = new BoundingBox(min.X, min.Y, max.X - min.X, max.Y - min.Y);
    }

    public override string SvgElement() {
        return string.Format(
            "<line x1=\"{0}\" y1=\"{1}\" x2=\"{2}\" y2=\"{3}\" stroke=\"{4}\" stroke-width=\"{5}\" stroke-opacity=\"{6}\" marker-start=\"{7}\" marker-end=\"{8}\"/>",
            Start.X, Start.Y,
            End.X, End.Y,
            ToHex(StrokeColour),
            StrokeWidth,
            StrokeColour.A,
            StartMarker,
            EndMarker
        );
    }

}

/// <summary>
/// 2d SVG rectangle
/// </summary>
public class Rect : SvgShape {

    public Rect(BoundingBox box) {
        this.Bounds = box;
    }

    public void Resize(BoundingBox box) {
        this.Bounds = box;
    }

    public override string SvgElement() {
        return string.Format(
            "<rect x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"{3}\" stroke=\"{4}\" stroke-opacity=\"{5}\" stroke-width=\"{6}\" fill=\"{7}\" fill-opacity=\"{8}\"/>",
            Bounds.MinX, Bounds.MinY,
            Bounds.Width, Bounds.Height,
            ToHex(StrokeColour),
            StrokeColour.A,
            StrokeWidth,
            ToHex(FillColour),
            FillColour.A
        );
    }
}

/// <summary>
/// 2d SVG circle
/// </summary>
public class Circle : SvgShape {
    public Vector2 Centre {get; set;}
    public float Radius {get; set;}
    public float Diametre {
        get {
            return Radius * 2;
        } 
        set {
            Radius = value / 2;
        }
    }

    public Circle(Vector2 position, float radius) {
        this.Centre = position;
        this.Radius = radius;
    }

    public override string SvgElement() {
        return string.Format(
            "<circle cx=\"{0}\" cy=\"{1}\" r=\"{2}\" stroke=\"{3}\" stroke-opacity=\"{4}\" stroke-width=\"{5}\" fill=\"{6}\" fill-opacity=\"{7}\"/>",
            Centre.X, Centre.Y, Radius,
            ToHex(StrokeColour),
            StrokeColour.A,
            StrokeWidth,
            ToHex(FillColour),
            FillColour.A
        );
    }
}

/// <summary>
/// Horizontal text alignment
/// </summary>
public enum HorizontalTextAnchor {
    start,
    middle,
    end
}

/// <summary>
/// Vertical text alignment
/// </summary>
public enum VerticalTextAnchor {
    hanging,
    middle,
    baseline,
}

/// <summary>
/// 2d SVG text
/// </summary>
public class Text : SvgShape {
    public Vector2 Position {get; private set;}
    public float Rotation {get; private set;}
    public string Value {get; private set;}

    public HorizontalTextAnchor HorizontalAnchor = HorizontalTextAnchor.start;
    public VerticalTextAnchor VerticalAnchor = VerticalTextAnchor.middle;

    public Text(Vector2 position, string text) {
        this.Position = position;
        this.Value = text;
    }

    public override string SvgElement() {
        return string.Format(
            "<text text-anchor=\"{9}\" alignment-baseline=\"{10}\" x=\"{0}\" y=\"{1}\" stroke=\"{2}\" stroke-opacity=\"{3}\" stroke-width=\"{4}\" fill=\"{5}\" fill-opacity=\"{6}\" rotation=\"{7}\">{8}</text>",
            Position.X, Position.Y,
            ToHex(StrokeColour),
            StrokeColour.A,
            StrokeWidth,
            ToHex(FillColour),
            FillColour.A,
            Rotation,
            System.Web.HttpUtility.HtmlEncode(this.Value),
            HorizontalAnchor.ToString(),
            VerticalAnchor.ToString()
        );
    }
}

/// <summary>
/// 2d SVG Polygon
/// </summary>
public class Polygon : SvgShape {
    public readonly List<Vector2> Points = new List<Vector2>();

    public Polygon() {}
    public Polygon(IEnumerable<Vector2> points) {
        Points.AddRange(points);
    }

    public override string SvgElement() {
        return string.Format(
            "<polygon points=\"{0}\" stroke=\"{1}\" stroke-opacity=\"{2}\" stroke-width=\"{3}\" fill=\"{4}\" fill-opacity=\"{5}\"/>",
            string.Join(' ', Points.Select(x => x.X + "," + x.Y)),
            ToHex(StrokeColour),
            StrokeColour.A,
            StrokeWidth,
            ToHex(FillColour),
            FillColour.A
        );
    }
}

/// <summary>
/// SVG definition
/// </summary>
public interface IDefinition {
    string SvgElement();
}

public enum MarkerUnits {
    strokeWidth
}

public enum MarkerOrientation {
    auto
}

public class MarkerDefintion : IDefinition {

    public string Id {get; set;}
    public BoundingBox ViewBox {get; set;}
    public int RefX {get; set;}
    public int RefY {get; set;}
    public int MarkerWidth {get; set;}
    public int MarkerHeight {get; set;}
    public SvgShape MarkerShape {get; set;}
    public MarkerUnits MarkerUnits {get; set;}
    public MarkerOrientation Orientation {get; set;}
    public MarkerDefintion (string id, BoundingBox viewBox) {
        this.Id = id;
        this.ViewBox = viewBox;
    }
    public string SvgElement() {
        return string.Format(
            "<marker id=\"{1}\" viewBox=\"{2}\" refX=\"{3}\" refY=\"{4}\" markerWidth=\"{5}\" markerHeight=\"{6}\" markerUnits=\"{7}\" orient=\"{8}\">{0}</marker>",
            MarkerShape.SvgElement(),
            Id,
            ViewBox.MinX + " " + ViewBox.MinY + " " + ViewBox.Width + " " + ViewBox.Height,
            RefX,
            RefY,
            MarkerWidth,
            MarkerHeight,
            MarkerUnits,
            Orientation
        );
    }
}

/// <summary>
/// Scalable vector graphic composed of many shapes
/// </summary>
public class Svg : IList<SvgShape> {
    private List<SvgShape> shapes = new List<SvgShape>();
    private List<IDefinition> definitions = new List<IDefinition>();

    public int Width => (int)Math.Ceiling(shapes.Select(x => x.Bounds?.MaxX ?? 0).Max());
    public int Height => (int)Math.Ceiling(shapes.Select(x => x.Bounds?.MaxY ?? 0).Max());

    public SvgShape this[int index] {
        get {
            return shapes[index];
        }
        set {
            shapes[index] = value;
        }
    }

    public int Count => shapes.Count;

    public bool IsReadOnly => false;

    public void Define(IDefinition definition) {
        this.definitions.Add(definition);
    }

    public void Add(SvgShape item){
        shapes.Add(item);
    }

    public void Clear() {
        shapes.Clear();
    }

    public bool Contains(SvgShape item) {
        return shapes.Contains(item);
    }

    public void CopyTo(SvgShape[] array, int arrayIndex) {
        shapes.CopyTo(array, arrayIndex);
    }

    public IEnumerator<SvgShape> GetEnumerator() {
        return shapes.GetEnumerator();
    }

    public int IndexOf(SvgShape item) {
        return shapes.IndexOf(item);
    }

    public void Insert(int index, SvgShape item) {
        shapes.Insert(index, item);
    }

    public bool Remove(SvgShape item) {
        return shapes.Remove(item);
    }

    public void RemoveAt(int index) {
        shapes.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator(){
        return shapes.GetEnumerator();
    }

    public void Stringify(TextWriter writer) {
        writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>");
        writer.WriteLine(string.Format("<svg width=\"{0}\" height=\"{1}\" xmlns=\"http://www.w3.org/2000/svg\">", Width, Height));
        
        // Write definitions
        if (definitions.Count > 0) {
            writer.WriteLine("<defs>");
            foreach (var def in definitions) {
                writer.WriteLine(def.SvgElement());
            }
            writer.WriteLine("</defs>");
        }

        // Write shapes
        foreach (var shape in shapes) {
            writer.WriteLine(shape.SvgElement());
        }

        writer.WriteLine("</svg>");
    }
}

}