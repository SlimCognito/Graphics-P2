using System;
using System.IO;

namespace Template {

    class Game
    {
        public Raytracer Tracer;
        bool Debugging;
	    // member variables 
	    public Surface Screen;
	    // initialize
	    public void Init()
	    {
            Light[] lights = new Light[1];
            lights[0] = new Light();
            Primitive[] primitives = new Primitive[4];
            primitives[0] = new Plane(new VPoint(0, 1, 0), -5);
            primitives[1] = new Sphere(new VPoint(0, 0, 5), 1);
            primitives[2] = new Sphere(new VPoint(-3, 0, 5), 1);
            primitives[3] = new Sphere(new VPoint(3, 0, 5), 1);
            //voeg de primitives toe
            Scene scene = new Scene(lights, primitives);
            Tracer = new Raytracer(scene, Screen);
            Debugging = true;
	    }
	    // tick: renders one frame
	    public void Tick()
	    {
		    Screen.Clear( 0 );
		    Screen.Print( "hello world", 2, 2, 0xffffff );
            Tracer.Render(Debugging);
	    }
    }
    
    public struct Ray
    {
        public VPoint Location;
        public VPoint Direction;
        public float Distance;
        public Ray(VPoint Locationinit, VPoint Directioninit, float DistanceInit)
        {
            Location = Locationinit;
            Direction = Directioninit;
            Distance = DistanceInit;
        }
        public void debug(Surface screen, VPoint endPoint)
        {
            screen.Line(Location.transform("x"), Location.transform("y"), endPoint.transform("x"), endPoint.transform("y"), 100);
        }
        public void debug(Surface screen, float length)
        {
            debug(screen, Location + Direction * length);
        }
    }

    class Camera
    {
        public VPoint Position = new VPoint(0, 0, 0);
        public VPoint Orientation = new VPoint(0, 0, 1);
        public VPoint Upperright = new VPoint(1, 1, 1);
        public VPoint upperleft, Lowerleft, Lowerright;

        public Ray getRay(float x,float y)
        {
            x /= 256;
            y /= 256;
            VPoint Direction = new VPoint(x -1, -(y -1), 0);
            Direction += Orientation;
            Direction = Direction.Normalize();
            return new Ray(Direction,Position,0);
        }
    }

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

        public VPoint transform()
        {
            VPoint Result = this;
            Result.Z -= 3;
            Result.Z += 5;
            Result.X += 5;
            Result.Z *= 51.2f;
            Result.X *= 51.2f;
            Result.Z = (int)Result.Z;
            Result.X = (int)Result.X;
            return Result;
        }

        public VPoint(float xinit, float yinit, float zinit)
        {
            X = xinit;
            Y = yinit;
            Z = zinit;
            RememberLength = -1;
        }
        public VPoint Normalize()
        {
            return new VPoint(X/Length, Y/Length, Z/Length);
        }
        public static float operator *(VPoint a, VPoint b)
        {
            return (a.X * b.X + a.Y * b.Y + a.Z * b.Z);
        }
        public static VPoint operator *(VPoint a, float l)
        {
            return new VPoint(a.X * l, a.Y * l, a.Z * l);
        }
        public static VPoint operator *(float l, VPoint b)
        {
            return new VPoint(l * b.X, l * b.Y, l * b.Z);
        }
        public static VPoint operator +(VPoint a, VPoint b)
        {
            return new VPoint(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        public static VPoint operator -(VPoint a, VPoint b)
        {
            return new VPoint(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
    }

    abstract class Primitive
    {
        abstract public float Intersect(Ray ray);
    }

    class Sphere : Primitive
    {
        public VPoint Location;
        public float Radius;
        public float Radius2;
        public Sphere(VPoint location, float radius)
        {
            Location = location;
            Radius = radius;
            Radius2 = radius * radius;
        }
        override public float Intersect(Ray ray) 
        {
            VPoint c = this.Location - ray.Location;
            float t = c * ray.Direction;
            VPoint q = c - t * ray.Direction;
            float p = q * q;
            if (p > this.Radius * this.Radius2) return -1;
            t -= (float) Math.Sqrt(this.Radius2 - p);
            ray.Distance = Math.Min(ray.Distance, Math.Max(0, t));
            return ray.Distance;
        }
    }

    class Plane : Primitive
    {
        public VPoint Normal;
        public float Distance;
        public Plane(VPoint normal, float distance)
        {
            Normal = normal;
            Distance = distance;
        }
        override public float Intersect(Ray ray)  //volgens mij werkt dit maar ik zou er gelukkig van worden als iemand dit checkt.
        {
            VPoint intersection = new VPoint(0, 0, 0);
            /*VPoint partialVector = ray.Direction * Normal;
            if (partialVector.length == 0 )
            {
                return intersection.length;
            }
            intersection = ray.Direction * (Distance / partialVector.length);
            intersection -= ray.Location * Normal;*/
            return intersection.Length;
        }
    }
    class Light
    {
        public VPoint Location;
        public float Red, Green, Blue;
    }

    class Scene
    {
        public Light[] Lights;
        public Primitive[] Primitives;

        public Scene(Light[] lights, Primitive[] primitives)
        {
            this.Lights = lights;
            this.Primitives = primitives;
        }

        public Intersection intersect(Ray ray)
        {
            return null;
        }
    }

    class Intersection
    {
        public Ray Ray;
        public Ray Normal;
        public VPoint Location;
        public Primitive ThingWeIntersectedWith;
        public void debug(Surface screen)
        {
            Ray.debug(screen, Location);
            Normal.debug(screen, 1);
        }
    }

    class Raytracer
    {
        public Scene Scene;
        public Camera Camera;
        public Surface Screen;

        public Raytracer(Scene scene, Surface screen)
        {
            this.Scene = scene;
            this.Screen = screen;
            this.Camera = new Camera();
        }

        public void Render(bool debugging)
        {
            Ray ray;
            for (int x = 0; x < Screen.width; x++)
            {
                for (int y = 0; y < Screen.height; y++)
                {
                    ray = Camera.getRay(x, y);
                    Intersection intersection = Scene.intersect(ray);

                    if (debugging)
                    {
                        if (intersection != null)
                            intersection.debug(Screen);
                        else
                            ;//smth
                    }

                }
            }
        }
        /* 
        public int Translate(float x, bool z)
        {
            int result = 0;
            if(z)
            {
                x -= 3;
            }
            x += 5;
            x *= 51.2f;
            result = (int)x;
            return result;
        }*/
    }
} // namespace Template