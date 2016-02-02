using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Path<Tile> : IEnumerable<Tile>
{

    public Tile LastStep { get; private set; }
    public Path<Tile> PreviousSteps { get; private set; }
    public double TotalCost { get; private set; }
    private Path(Tile lastStep, Path<Tile> previousSteps, double totalCost)
    {
        LastStep = lastStep;
        PreviousSteps = previousSteps;
        TotalCost = totalCost;
    }
    public Path(Tile start) : this(start, null, 0) { }
    public Path<Tile> AddStep(Tile step, double stepCost)
    {
        return new Path<Tile>(step, this, TotalCost + stepCost);
    }
    public IEnumerator<Tile> GetEnumerator()
    {
        for (Path<Tile> p = this; p != null; p = p.PreviousSteps)
            yield return p.LastStep;
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
