This is a little demo to help me explore some super cool pathfinding algorithms. Part and parcel of this process of learning is good visualization techniques, and so I have implemented the demo in [Unity3D](https://unity.com/solutions/game) to make the abstract algorithms concrete.

![Pathfinder](/Docs/Pathfinder.PNG?raw=true)

# How to Run

Open this project in Unity and simply press Play.

This project was made using Unity version [2018.4.10](unityhub://2018.4.10f1/a0470569e97b). It probably wouldn't hurt to try opening it in a newer or older version though - YMMV.

## Tweakable Parameters

Click on the `World` GameObject in order to access some options in the Inspector that will vary the behavior of the algorithms.

![Select the World GameObject in the Heirarchy view](/Docs/SelectWorld.PNG?raw=true "Select the World GameObject in the Heirarchy view")
![Tweakable Options](/Docs/TweakableOptions.PNG?raw=true "Tweakable Options")

### Cell Size

I am currently focusing on tile/grid-based maps for experimenting with pathfinding. The cell size is the length of a side of a square-shaped grid cell in Unity units. Based on this value, the World is transformed into a grid structure from which a Graph data structure can be built to perform pathfinding on.

It determines the size of the grid. For example, a World that is 10 by 10 units and a cell size of 0.5 will result in the construction of a 20 by 20 grid containing 400 cells.

The range of possible values is [0.05, 1]. Any more or less and you either get terrible pathing or terrible performance.

![Cell Size 1](/Docs/Cell-1.00.PNG?raw=true "Cell Size 1")

![Cell Size 0.5](/Docs/Cell-0.50.PNG?raw=true "Cell Size 0.5")

![Cell Size 0.25](/Docs/Cell-0.25.PNG?raw=true "Cell Size 0.25")

![Cell Size 0.1](/Docs/Cell-0.10.PNG?raw=true "Cell Size 0.1")

### End Goal Heuristic

Choose the heuristic function to be used by the A* algorithm.

![Manhattan Distance](/Docs/Manhattan.PNG?raw=true "Manhattan Distance")

![Euclidean Distance](/Docs/Euclidean.PNG?raw=true "Euclidean Distance")

## Obstacles

The demo supports dynamic obstacle detection at tilemap generation time. You can add/remove/move objects with Collider components on the Ground object, then press Play to see them being considered as impassable tiles during the pathfinding algorithm!

![Obstacles 1](/Docs/Obstacles1.PNG?raw=true)

![Obstacles 2](/Docs/Obstacles2.PNG?raw=true)

![Obstacles 3](/Docs/Obstacles3.PNG?raw=true)

# How it Works

Pathfinding is performed on a Ground GameObject that must have some sort of a Collider component. For simplicity, it is assumed to be an object that is spread out along the X-Z plane such that the Y-axis is insignificant. Slopes, inclines and general rugged terrain is currently not supported. I chose to use a Plane but a stretched out Cube should get the job done too.

The Ground surface is split across the X-Z plane into a 2D grid, aka tilemap. Tiles that overlap with an object with a Collider are marked as obstructing tiles and will be rendered as impassable during pathfinding.

![Tilemap](/Docs/Tilemap.PNG?raw=true "Visualization of the grid/tilemap - Note the obstacle tiles represented by red spheres")

Left-click anywhere on the Ground to choose the starting location. Right-click anywhere else on the Ground to choose the ending location. As soon as this is done, the pathfinding algorithm will compute a path from the starting location to the ending location and this path is then rendered on the Ground. Start and end locations can be changed freely to explore different paths.

Press and hold the middle-mouse button and drag around to pan the camera.

![Controls](/Docs/Controls.gif?raw=true)

# Acknowledgements

[Tim Roughgarden's](https://www.amazon.ca/Algorithms-Illuminated-Part-Graph-Structures-ebook/dp/B07G6X2XMG/ref=sr_1_3?keywords=algorithms+tim+roughgarden&qid=1573363767&sr=8-3) Algorithms Illuminated textbook. Prior to working through this book I was intimidated by Graph algorithms, particularly Djikstra's single-source shortest path algorithm. That is no longer the case!

[Amit Patel's](https://www.redblobgames.com/pathfinding/a-star/introduction.html) splendid pathfinding algorithm illustrations and sample code have been extremely insightful. The interactive explanations helped me grasp the intuition of Djikstra's and A* algorithms. This is a gem of a resource!

# TODO

There are many more aspects of pathfinding that I will be exploring.

- Move a 3D object along a computed path.
- Add a new heuristic function: Precomputed shortest path distance to the end goal.
- Smoothly move a 3D object along a computed path, even when the path looks jagged and ugly.