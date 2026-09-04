using UnityEngine;
using VContainer;
using VContainer.Unity;

public class InGameLifetimeScope : LifetimeScope
{ 
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<Table>();
        builder.RegisterComponentInHierarchy<PlayerController>();
        builder.RegisterComponentInHierarchy<Gauge>();
        builder.RegisterComponentInHierarchy<AutoSolveController>();
        builder.Register<DFS>(Lifetime.Singleton).As<IAlgorithm>();
        builder.Register<AStar>(Lifetime.Singleton);
    }
}
