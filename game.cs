using System;
using System.IO;
using System.Drawing;

namespace Template { 
    class Game
    {
        // Debugging tells us whether we are debugging or not
        public bool           Debugging;
        public static int     Recursion;
	    public Surface        Screen;
        public static float[] SinTable;
        public static Bitmap  Space;
        public Raytracer      Tracer;
	    // initialize
	    public void Init()
	    {
            // Set recursion level
            Recursion = 5;
            // Add light(s)
            Light[] lights = new Light[2];
            lights[0] = new Light(new VPoint(1, 2, 0), 1, 1, 1);
            lights[1] = new Light(new VPoint(2, 10, 5), 2, 2, 2);
            // Add primitive(s)
            Primitive[] primitives = new Primitive[5];
            primitives[0] = new Plane(new VPoint(0, 1, 0), -2, new Material(0.5f,1));
            primitives[1] = new Sphere(new VPoint(0, 0, 5), 1.5f, new Material(new VPoint(255, 50, 100), 0.5f));
            primitives[2] = new Sphere(new VPoint(-3, 0, 5), 1.5f, new Material(new VPoint(0, 255, 10), 0.5f));
            primitives[3] = new Sphere(new VPoint(3, 0, 5), 1.5f, new Material(new VPoint(255, 255, 255), 0.75f));
            // The sphere in which the whole scene is situated
            primitives[4] = new Sphere(new VPoint(0, -5, 1), 10f, new Material(0f,2));
            // Create scene
            Scene scene = new Scene(lights, primitives);
            // Create raytracer
            Tracer = new Raytracer(scene, Screen);
            // Set debugging
            Debugging = true;
            // Calculating the sinus every time you need is, is slow, as such we calculate the values once and use an array
            SinTable = new float[360];
            for (int i = 0; i < 360; i++)
                SinTable[i] = (float)Math.Sin(i * Math.PI / 180);
            //load bitmap4space
            string s = Path.Combine(Environment.CurrentDirectory, @"Data\", "Space.jpg");
            Space = new Bitmap(s);
	    }
	    // tick: renders one frame
	    public void Tick()
	    {
            // Handle keyboard input. Arrow keys move the camera, WASD is used for turning the camera, LShift and Enter move the camera up and down
            var keyboard = OpenTK.Input.Keyboard.GetState();
            if (keyboard[OpenTK.Input.Key.W])
                Tracer.Camera.turnCamera(-0.1f * Tracer.Camera.YDirection);
            if (keyboard[OpenTK.Input.Key.A])
                Tracer.Camera.turnCamera(-0.1f * Tracer.Camera.XDirection);
            if (keyboard[OpenTK.Input.Key.S])
                Tracer.Camera.turnCamera(0.1f * Tracer.Camera.YDirection);
            if (keyboard[OpenTK.Input.Key.D])
                Tracer.Camera.turnCamera(0.1f * Tracer.Camera.XDirection);
            if (keyboard[OpenTK.Input.Key.Enter])
                Tracer.Camera.moveCamera(-0.1f * Tracer.Camera.YDirection);
            if (keyboard[OpenTK.Input.Key.LShift])
                Tracer.Camera.moveCamera(0.1f * Tracer.Camera.YDirection);
            if (keyboard[OpenTK.Input.Key.Left])
                Tracer.Camera.moveCamera(-0.1f * Tracer.Camera.XDirection);
            if (keyboard[OpenTK.Input.Key.Right])
                Tracer.Camera.moveCamera(0.1f * Tracer.Camera.XDirection);
            if (keyboard[OpenTK.Input.Key.Up])
                Tracer.Camera.moveCamera(0.1f * Tracer.Camera.Orientation);
            if (keyboard[OpenTK.Input.Key.Down])
                Tracer.Camera.moveCamera(-0.1f * Tracer.Camera.Orientation);
            Screen.Clear(0);
            Screen.Print("Tracer", 2, 2, 0xffffff);
            Tracer.Render(Debugging);
        }
    }

    public class Material
    {
        public VPoint Color;
        public int    Texture;
        public float  Reflects;
        public VPoint GetColor(VPoint p)
        {
            switch(Texture)
            {
                case 0: return Color;
                case 1: return new VPoint(231, 231, 231) * ((((Math.Abs((int)Math.Floor(p.X) + (int)Math.Floor(p.Z)))) % 2) + 0.1f);
                case 2:
                    {
                        int x = Modulo((int)(p.X * 600)+3000,6000);
                        int y = Modulo((int)(p.Z * 400)+1600,4000);
                        Color Pixel = Game.Space.GetPixel(x, y);
                        return new VPoint(Pixel.R, Pixel.G, Pixel.B);
                    }
                default: return Color;
            }
        }
        //better modulo
        int Modulo(int i, int y)
        {
            if ((i / y) % 2 == 1)
                i = -i;
            while (i < 0)
                i += y;
            while (i >= y)
                i -= y;
            return i;
        }
        // Create material
        public Material(float r, int t)
        {
            Texture = t;
            Reflects = r;
        }
        // Create reflective material
        public Material(VPoint c, float r)
        {
            Color = c;
            Reflects = r;
            Texture = 0;
        }
    }

    // X,Y,Z = coordinates, RememberLength = used to determine whether the lenght has been calculated yet. If not the length will be calculated.
    // RememberLength has been implemented with the idea that we won't always need the length of the vector, especially since a VPoint is also
    // used to store colors (as (R, G, B)).
    public struct VPoint
    {
        public float X;
        public float Y;
        public float Z;
        public float RememberLength;
        public float Length
        {
            get
            {
                if (RememberLength == -1)
                    RememberLength = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
                return RememberLength;
            }
        }
        // Used in debugging window, if the string != "x" it transforms to the Y-coordinate on your screen.
        public int transform(string coordinate)
        {
            if (coordinate == "x")
            {
                return (int)((5 + X) * 51.2f);
            }
            return 512 - (int)((Z + 2) * 51.2f);
        }
        // 3D vector / point in 3D space / Color in R, G, B values
        public VPoint(float xinit, float yinit, float zinit)
        {
            X = xinit;
            Y = yinit;
            Z = zinit;
            RememberLength = -1;
        }
        // Normalize returns a new vector, so that if needed we still have the old vector.
        public VPoint Normalize()
        {
            return new VPoint(X / Length, Y / Length, Z / Length);
        }
        // Dotproduct
        public static float operator *(VPoint a, VPoint b)
        {
            return (a.X * b.X + a.Y * b.Y + a.Z * b.Z);
        }
        // Crossproduct
        public static VPoint operator %(VPoint a, VPoint b)
        {
            return new VPoint((a.Y * b.Z) - (a.Z * b.Y), (a.Z * b.X) - (a.X * b.Z), (a.X * b.Y) - (a.Y * b.X));
        }
        // Multiplication with scalar
        public static VPoint operator *(VPoint a, float l)
        {
            return new VPoint(a.X * l, a.Y * l, a.Z * l);
        }
        // Multiplication with scalar
        public static VPoint operator *(float l, VPoint b)
        {
            return new VPoint(l * b.X, l * b.Y, l * b.Z);
        }
        // Vector addition
        public static VPoint operator +(VPoint a, VPoint b)
        {
            return new VPoint(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        // Vector substraction
        public static VPoint operator -(VPoint a, VPoint b)
        {
            return new VPoint(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        // Determine the color based on reflection and diffusion
        public static VPoint colorStuff(VPoint reflection, VPoint diffusion, float r)
        {
            return (diffusion * (1- r) + r * new VPoint((diffusion.X * reflection.X) / 255, (diffusion.Y * reflection.Y) / 255, (diffusion.Z * reflection.Z) / 255));
        }
        // Get a color based on the R, G, B values
        public int getColor()
        {
            int color = (int)((int)X * 256 * 256 + (int)Y * 256 + (int)Z);
            string scolor = color.ToString("X");
            return Convert.ToInt32(scolor, 16);
        }
    }

    // Location = ray origin, Direction = ray direction and Distance = ray length
    public struct Ray
    {
        public VPoint Location;
        public VPoint Direction;
        public float  Distance;
        public int recursion;
        // Create ray
        public Ray(VPoint Locationinit, VPoint Directioninit)
        {
            Location = Locationinit;
            Direction = Directioninit.Normalize();
            Distance = float.PositiveInfinity;
            recursion = 0;
        }
        public void debug(Surface screen, VPoint endPoint, int type)
        {
            int color = 0;
            switch (type)
            {
                case 0:
                    color = 0xFF0000;
                    break;
                case 1:
                    color = 0x00FF00;
                    break;
                case 2:
                    color = 0x0000FF;
                    break;
                case 3:
                    color = 0xFFFFFF;
                    break;
                case 4:
                    color = 0x888800;
                    break;
            }
            // Make sure that the debug output isn't drawn in the scene (in other words, limit the x value to 512, if it exceeds 512 don't draw the line in question)
            int x1, x2;
            x1 = Location.transform("x");
            x2 = endPoint.transform("x");
            if (x1 <= 512 && x2 <= 512)
            {
                screen.Line(x1, Location.transform("y"), x2, endPoint.transform("y"), color);
            }
        }
        public void debug(Surface screen, float length, int type)
        {
            debug(screen, Location + Direction * length, type);
        }
    }

    // Position = camera position, Orientation = camera direction, Upperleft etc = upperleft corner etc., 
    // X-,YDirection = direction to move in when changing X/Y, eg when moving from upperleft to upperright we add to X,
    // moving from upperleft to lowerleft means we add to Y.
    class Camera
    {
        public VPoint Position;
        public VPoint Orientation;
        public VPoint Upperleft;
        public VPoint XDirection;
        public VPoint YDirection;
        public VPoint Upperright, Lowerleft, Lowerright;
        public VPoint Target;
        // Create camera
        public Camera()
        {
            Position = new VPoint(0, 0, 0);
            Orientation = new VPoint(0, 0, 1);
            Target = Position + Orientation;
            Upperleft = new VPoint(-1, 1, 1);
            XDirection = new VPoint(1, 0, 0);
            YDirection = new VPoint(0, -1, 0);
            Upperright = new VPoint(1, 1, 1);
            Lowerleft = new VPoint(-1, -1, 1);
            Lowerright = new VPoint(1, -1, 1);
        }
        //To translate the camera
        public void moveCamera(VPoint direction)
        {
            Position += direction;
            Upperleft += direction;
            Upperright += direction;
            Lowerleft += direction;
            Lowerright += direction;
            Target += direction;
        }
        //Sets the horizontal direction of the virtual screen after the camera is rotated.
        private void setXDirection()
        {
            float dX;
            if (Orientation.Z > 0)
            {
                dX = 1;
            }
            else if (Orientation.Z < 0)
            {
                dX = -1;
            }
            else
            {
                XDirection = new VPoint(0, 0, -Orientation.X).Normalize();
                return;
            }
            XDirection = new VPoint(dX, 0, -Orientation.X * dX / Orientation.Z).Normalize();
        }
        //Sets the vertical direction of the virtual screen after the camera is rotated.
        private void setYDirection()
        {
            YDirection = XDirection % Orientation;
            if (YDirection.Y > 0)
                YDirection *= -1;
        }
        //To rotate the camera
        public void turnCamera(VPoint direction)
        {
            Orientation = (Orientation + direction).Normalize();
            Target = Position + Orientation;
            setXDirection();
            setYDirection();
            Upperleft = Target - XDirection - YDirection;
            Upperright = Target + XDirection - YDirection;
            Lowerleft = Target - XDirection + YDirection;
            Lowerright = Target + XDirection - YDirection;
        }
        // Get a ray through given coordinates
        public Ray getRay(float x,float y)
        {
            x /= 256;
            y /= 256;
            VPoint positionOnScreen = Upperleft;
            positionOnScreen += x * XDirection + y * YDirection;
            return new Ray(Position, (positionOnScreen - Position).Normalize());
        }
        public void debug(Surface screen)
        {
            int x1, x2;
            x1 = Upperleft.transform("x");
            x2 = Upperright.transform("x");
            if (x1 <= 512 && x2 <= 512)
                screen.Line(x1, Upperleft.transform("y"), x2, Upperright.transform("y"), 255255255);
        }
    }
   
    abstract class Primitive
    {
        public Material Mat;
        abstract public Ray normal(VPoint location);
        abstract public void debug(Surface screen);
        abstract public float Intersect(Ray ray); // Misschien naar abstract public void Intersect en de intersection opslaan in class Intersect?
        // Reflect a ray
        public Ray Reflect(Ray ray, VPoint location)
        {
            VPoint d = ray.Direction;
            VPoint n = normal(location).Direction;
            return new Ray(location, (d - (2 * (d * n) * n)).Normalize());
        }
    }

    class Sphere : Primitive
    {
        // Radius2 = Radius^2, Location = centre of the sphere
        public VPoint Location;
        public float  Radius;
        public float  Radius2;
        // Create sphere
        public Sphere(VPoint location, float radius, Material mat)
        {
            Mat = mat;
            Location = location;
            Radius = radius;
            Radius2 = radius * radius;
        }
        public override void debug(Surface screen)
        {
            float newradius = (float)Math.Sqrt(Radius2 - Location.Y);
            VPoint middle = Location;
            middle.Y = 0;
            VPoint previousDrawPoint = new VPoint(0, 0, 1);
            previousDrawPoint = previousDrawPoint.Normalize()*Radius;
            previousDrawPoint += middle;
            for(int i = 1; i<121; i++)
            {
                VPoint DrawPoint = new VPoint(Game.SinTable[(i * 3) % 360], 0, Game.SinTable[(i * 3 + 90) % 360]);
                DrawPoint = DrawPoint.Normalize()*Radius;
                DrawPoint += middle;
                int x1, x2;
                x1 = DrawPoint.transform("x");
                x2 = previousDrawPoint.transform("x");
                if (x1 <= 512 && x2 <= 512)
                    screen.Line(x1, DrawPoint.transform("y"), x2, previousDrawPoint.transform("y"), Mat.GetColor(new VPoint(0, 0, 0)).getColor());
                previousDrawPoint = DrawPoint;
            }

        }
        // Intersects with a ray, returns the length at which the ray hits the sphere, -1 if no intersection
        override public float Intersect(Ray ray) 
        {
            // If the ray starts inside of the sphere
            if ((Location - ray.Location).Length < Radius-0.001f)
            {
                float a = ray.Direction*ray.Direction;
                float b = ray.Direction * ( ray.Location -Location ) * 2;
                float c = (ray.Location -Location)*(ray.Location - Location) - Radius2;
                float d = b*b - 4*a*c;
                d = (float)Math.Sqrt(d);
                float distance = Math.Max( (-b+d)/(2*a) , (-b-d)/(2*a));
                return distance;
            }
            // Otherwise the ray starts outside of the sphere
            else
            {
                VPoint c = Location - ray.Location;
                float t = c * ray.Direction;
                VPoint q = c - t * ray.Direction;
                float p = q * q;
                if (p > Radius2) return -1;
                t -= (float)Math.Sqrt(Radius2 - p);
                return t;
            }
        }
        // Normal on the sphere given the location of the intersection
        public override Ray normal(VPoint location)
        {
            return new Ray(location, (location - Location).Normalize());
        }        
    }

    // Plane is determined by normal and distance to the origin.
    class Plane : Primitive
    {
        public VPoint Normal;
        public float  Distance;
        // Create the plane
        public Plane(VPoint normal, float distance, Material mat)
        {
            Mat = mat;
            Normal = normal.Normalize();
            Distance = distance;
        }
        // Serves no purpose, but has to be included because it is a primitive
        public override void debug(Surface screen)
        {
        }
        // Ray - Plane intersection
        override public float Intersect(Ray ray)
        {
            if (Normal * ray.Direction == 0)
                return -1;
            else
                return (((Distance - ray.Location * Normal) / (Normal * ray.Direction)) * ray.Direction.Length);
        }
        // Since the plane is defined by its normal and distance to the origin, we simply return the normal as a ray
        public override Ray normal(VPoint location)
        {
            return new Ray(location, Normal);
        }
    }

    class Light
    {
        // Location = location of the light source, Red, Green and Blue determine the light intensity
        public VPoint Location;
        public float  Red, Green, Blue;
        // Create a light source
        public Light(VPoint location, float r, float g, float b)
        {
            Location = location;
            Red = r;
            Green = g;
            Blue = b;
        }
        // Determine the reflected color
        public VPoint reflectedColor(VPoint colorOfObject, float intensity)
        {
            return new VPoint((int)(colorOfObject.X * intensity*Red),(int)( colorOfObject.Y * intensity*Green), (int)(colorOfObject.Z * intensity*Blue));
        }
    }

    class Scene
    {
        // A scene is a list of lights and primitives
        // The intersection methods checks for each ray whether it intersects with a primitive
        public Light[]     Lights;
        public Primitive[] Primitives;
        // Create the scene
        public Scene(Light[] lights, Primitive[] primitives)
        {
            this.Lights = lights;
            this.Primitives = primitives;
        }
        // Check for intersections
        public Intersection intersect(Ray ray)
        {
            Primitive Hit = null;
            float j;
            foreach(Primitive p in Primitives)
            {
                j = p.Intersect(ray);
                if(j < ray.Distance && j > 0)
                {
                    ray.Distance = j;
                    Hit = p;
                }
            }
            return new Intersection(ray, ray.Location + ray.Direction*ray.Distance, Hit);
        }
    }

    class Intersection
    {
        // An intersection is determined by the ray that intersects at a location with a certain primitive
        public Ray       Ray, secondaryRay;
        public Ray[]     ShadowRays;
        public VPoint    Location;
        public Primitive ThingWeIntersectedWith;
        public float     Distance;
        // Create an intersection
        public Intersection(Ray ray,  VPoint location, Primitive p)
        {
            Distance = (location - ray.Location).Length;
            Ray = ray;
            Location = location;
            ThingWeIntersectedWith = p;
        }
        // Determine the colors on the scene
        public VPoint color(Scene scene)
        {
            if (ThingWeIntersectedWith != null)
            {
                ShadowRays = new Ray[scene.Lights.Length];
                VPoint diffusion = new VPoint();
                for(int i = 0; i < scene.Lights.Length; i++)
                {
                    Light light = scene.Lights[i];
                    VPoint shadowRayDirection = (light.Location - Location);
                    ShadowRays[i] = new Ray(Location + 0.00001f * shadowRayDirection.Normalize(), shadowRayDirection.Normalize());
                    ShadowRays[i].Distance = shadowRayDirection.Length;
                    float distance = scene.intersect(ShadowRays[i]).Distance;
                    if (distance >= shadowRayDirection.Length - 2 * 0.00001)
                    {
                        ShadowRays[i].Distance = distance;
                        VPoint j = ThingWeIntersectedWith.normal(Location).Direction;
                        if (j * Ray.Direction > 0)
                            j *= -1;
                        diffusion += light.reflectedColor(ThingWeIntersectedWith.Mat.GetColor(Location), 60*Math.Max(0, j * ShadowRays[i].Direction.Normalize()) * (1 / (shadowRayDirection.Length * shadowRayDirection.Length)));
                    }
                }
                diffusion = new VPoint(Math.Min(diffusion.X, 255), Math.Min(diffusion.Y, 255), Math.Min(diffusion.Z, 255));
                if (ThingWeIntersectedWith.Mat.Reflects != 0 && Ray.recursion < Game.Recursion)
                {
                    secondaryRay = ThingWeIntersectedWith.Reflect(Ray, Location);
                    secondaryRay.recursion = Ray.recursion + 1;
                    Intersection inter = scene.intersect(secondaryRay);
                    if (inter.ThingWeIntersectedWith == scene.Primitives[0])
                        secondaryRay.Distance = 3;
                    else
                        secondaryRay.Distance = inter.Distance;
                    return VPoint.colorStuff(inter.color(scene), diffusion, ThingWeIntersectedWith.Mat.Reflects);
                }
                else
                    return diffusion;
            }
            return new VPoint();
        }

        public void debug(Surface screen) 
        {
            Ray.debug(screen, Location, 0);
            if (ThingWeIntersectedWith != null)
            {
                Ray j = ThingWeIntersectedWith.normal(Location);
                if (j.Direction * Ray.Direction > 0)
                    j.Direction = j.Direction * -1;
                j.debug(screen, 1, 2);
            }
            if (ShadowRays != null)
            {
                foreach (Ray shadowRay in ShadowRays)
                    shadowRay.debug(screen, shadowRay.Distance, 4);
            }
            if (ThingWeIntersectedWith != null && ThingWeIntersectedWith.Mat.Reflects > 0)
                secondaryRay.debug(screen, secondaryRay.Distance, 1);
        }
    }

    class Raytracer
    {
        // The raytracer is a combination of the scene to be traced, the camera and the screen
        public Scene   Scene;
        public Camera  Camera;
        public Surface Screen;
        // Create the raytracer
        public Raytracer(Scene scene, Surface screen)
        {
            Scene = scene;
            Screen = screen;
            Camera = new Camera();
        }
        // Render the scene
        public void Render(bool debugging)
        {
            Ray ray;
            for (int y = 0; y < Screen.height; y++)
            {
                for (int x = 0; x < Screen.width / 2; x++)
                {
                    ray = Camera.getRay(x, y);
                    Intersection intersection = Scene.intersect(ray);

                    Screen.pixels[x + Screen.width / 2 + y * Screen.width] = intersection.color(Scene).getColor();
                    if (debugging && y == 256 && x % 50 == 0)
                    {
                        if (intersection.ThingWeIntersectedWith != Scene.Primitives[4])
                            intersection.debug(Screen);
                        else
                            ray.debug(Screen, 7, 3);
                    }
                }
                if (debugging)
                {
                    foreach (Primitive p in Scene.Primitives)
                        p.debug(Screen);
                    Camera.debug(Screen);
                }
            }
        }
    }
}