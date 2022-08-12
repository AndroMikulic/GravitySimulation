
# Unity Gravity Simulation

This little Unity project simulates multibody gravity using a compute shader that uses [Newton's formula for gravity](https://www.britannica.com/science/Newtons-law-of-gravitation)

### The setup menu
![Unity Gravity Simulation menu](https://i.postimg.cc/8PPHHRqY/menu.png)
#### Parameters explained
- Amount of bodies - total amounf of phsysics objects that will be simulated.
- Maximum mass - the maximum mass that the randomizer will use when generating bodies. The mass of all bodies will always be [1, Maximum mass].
- Size modifier - the maximum size modifier for the most massive objects.
- Spread modifier - the edge length of the square that the bodies will spawn in (basically, if you put 1024, the bodies will spawn in a 1024*1024 unit area)
- Velocity modifier - each body is given a random direction to move towards. The velocity modifier is aplifies the velocity.
- Gravitational constant - the G variable used in Newton's gravity formula. This is not physically accurate since the masses are really small. 

Try playing around with the values. Even slight modifications can give very varying results!
##
In Game
![Unity Gravity Simulation](https://i.postimg.cc/dtmb7jGS/ingame.png)
#### Controls
- Key: M - toggles between MANUAL and AUTOMATIC mode.
- Key: Q - zoom in.
- Key: E - zoom out.
- Key: ESC - reset the simuation, returns to the main screen.
- Mouse: movement - Move around (when in manual mode)

Each body is represneted by size and color. The smallest and yellowest bodies are the lightest, while the reddest and biggest bodies are the heaviest.

For each body, the gravitational force is calculated by iterating over every other body and calculating the gravitational pull for it.
All of the vectors are summed up and a resulting vector is given for 1 body.
This process is exteremly CPU heavy, but when written in a compute shader and given to the graphics card, the performance gains are huge and immediately noticable. 

To see the difference for yourself, clone the project and run it with eat least 1024 bodies. Check it out for a minute or two then hit escape. Then, click on the Simulation Manager in the inspector and un-check the "Use Compute Shader" boolean - this will result in the vector calculations being done on the CPU.

![Use Compute Shader checkbox](https://i.postimg.cc/pr3gDsqg/compute-shader-toggle.png)

The compute shader and it's manager can be found in [Assets/Compute Shader](Assets/Compute%20Shaders/) folder.
The compute shader that calculates the total force vector on a single body is called [ForceCalculation.compute](Assets/Compute%20Shaders/ForceCalculation.compute).
A manager is required to handle handing over and reading buffer data, as well as applying Unity's physics to each Rigidbody in the scene. That file is [ComputeShaderManager.cs](Assets/Compute%20Shaders/ComputeShaderManager.cs)


