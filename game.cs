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
            lights[0] = new Light(new VPoint(0, 0, 0), 1, 1, 1);
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
        public Ray(VPoint Locationinit, VPoint Directioninit)
        {
            Location = Locationinit;
            Direction = Directioninit;
            Distance = 20;
        }
        public void debug(Surface screen, VPoint endPoint)
        {
            screen.Line(Location.transform("x"), Location.transform("y"), endPoint.transform("x"), endPoint.transform("y"), 100);
        }
        public void debug(Surface screen, float length)
        {
            VPoint d = Direction.Normalize();
            debug(screen, Location + d * length);
        }
    }

    class Camera
    {
        public VPoint Position = new VPoint(0, 0, 0);
        public VPoint Orientation = new VPoint(0, 0, 1);
        public VPoint Upperleft = new VPoint(-1, 1, 1);
        public VPoint XDirection = new VPoint(1, 0, 0);
        public VPoint YDirection = new VPoint (0, -1, 0);
        public VPoint upperright, Lowerleft, Lowerright;

        public Ray getRay(float x,float y)
        {
            x /= 256;
            y /= 256;
            //VPoint Direction = new VPoint(x -1, -(y -1), 0);
            //Direction += Orientation;
            //Direction = Direction.Normalize();
            //return new Ray(Position, Direction,0);
            VPoint positionOnScreen = new VPoint(Upperleft.X, Upperleft.Y, Upperleft.Z);
            positionOnScreen += x * XDirection + y * YDirection;
            return new Ray(Position, (positionOnScreen - Position).Normalize());
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

        public int transform(string coordinate)
        {
            if (coordinate == "x")
            {
                return (int)((5+X)*51.2f);
            }
            return 512 - (int)((Z + 2) * 51.2f);
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
        abstract public Ray normal(VPoint location);
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
        public override Ray normal(VPoint location)
        {
            throw new NotImplementedException();
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
            if (Normal * ray.Direction == 0)
                return -1;
            else
                return (((Distance - ray.Location * Normal) / (Normal * ray.Direction)) * ray.Direction.Length);

        }
        public override Ray normal(VPoint location)
        {
            return new Ray(location, Normal);
        }
    }
    class Light
    {
        public VPoint Location;
        public float Red, Green, Blue;
        
        public Light(VPoint location, float r, float g, float b)
        {
            Location = location;
            Red = r;
            Green = g;
            Blue = b;
        }
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
            Primitive Hit = null;
            float j;
            foreach(Primitive p in Primitives)
            {
                j = p.Intersect(ray);
                if(j< ray.Distance&&j!=-1)
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
        public Ray Ray;
        public VPoint Location;
        public Primitive ThingWeIntersectedWith;

        public Intersection(Ray ray,  VPoint Location, Primitive p)
        {
            this.Ray = ray;
            this.Location = Location;
            this.ThingWeIntersectedWith = p;
        }

        public void debug(Surface screen)
        {
            Ray.debug(screen, Location);
            ThingWeIntersectedWith.normal(Location).debug(screen, 1);
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

                    if (debugging && y == 0 && x % 20 == 0)
                    {
                        if (intersection != null)
                            intersection.debug(Screen);
                        else
                            ray.debug(Screen, 8);//smth
                    }

                }
            }
        }
    }
}