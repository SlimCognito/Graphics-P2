using System;
using System.IO;

namespace Template {

    class Game
    {
        public Raytracer tracer;
        bool debugging;
	    // member variables 
	    public Surface screen;
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
            tracer = new Raytracer(scene, screen);
            debugging = true;
	    }
	    // tick: renders one frame
	    public void Tick()
	    {
		    screen.Clear( 0 );
		    screen.Print( "hello world", 2, 2, 0xffffff );
            tracer.Render(debugging);
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

        }
        public void debug(Surface screen, float length)
        {
            debug(screen, Location + Direction * length);
        }
    }

    class Camera
    {
        public VPoint position = new VPoint(0, 0, 0);
        public VPoint orientation = new VPoint(0, 0, 1);
        public VPoint upperright = new VPoint(1, 1, 1);
        public VPoint upperleft, lowerleft, lowerright;

        public Ray getRay(float x,float y)
        {
            x /= 512;
            y /= 512;
            VPoint Direction = new VPoint(x /= 256 -1, -(y /= 256 -1), 0);
            Direction += orientation;
            Direction = Direction.Normalize();
            return new Ray(Direction,position,0);
        }
    }

    public struct VPoint
    {
        public float x;
        public float y;
        public float z;
        public float rememberLength;
        public float length
        {
            get
            {
                if (rememberLength == -1)
                    rememberLength = (float)Math.Sqrt(x * x + y * y + z * z);
                return rememberLength;
            }
        }

        public VPoint(float xinit, float yinit, float zinit)
        {
            x = xinit;
            y = yinit;
            z = zinit;
            rememberLength = -1;
        }
        public VPoint Normalize()
        {
            return new VPoint(x/length, y/length, z/length);
        }
        public static float operator *(VPoint a, VPoint b)
        {
            return (a.x * b.x + a.y * b.y + a.z * b.z);
        }
        public static VPoint operator *(VPoint a, float l)
        {
            return new VPoint(a.x * l, a.y * l, a.z * l);
        }
        public static VPoint operator *(float l, VPoint b)
        {
            return new VPoint(l * b.x, l * b.y, l * b.z);
        }
        public static VPoint operator +(VPoint a, VPoint b)
        {
            return new VPoint(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static VPoint operator -(VPoint a, VPoint b)
        {
            return new VPoint(a.x - b.x, a.y - b.y, a.z - b.z);
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
            return intersection.length;
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
        public VPoint location;
        public Primitive ThingWeIntersectedWith;
        public void debug(Surface screen)
        {
            Ray.debug(screen, location);
            Normal.debug(screen, 1);
        }
    }

    class Raytracer
    {
        public Scene scene;
        public Camera camera;
        public Surface screen;

        public Raytracer(Scene scene, Surface screen)
        {
            this.scene = scene;
            this.screen = screen;
            this.camera = new Camera();
        }

        public void Render(bool debugging)
        {
            Ray ray;
            for (int x = 0; x < screen.width; x++)
            {
                for (int y = 0; y < screen.height; y++)
                {
                    ray = camera.getRay(x, y);
                    Intersection intersection = scene.intersect(ray);

                    if (debugging)
                    {
                        if (intersection != null)
                            intersection.debug(screen);
                        else
                            ;//smth
                    }

                }
            }
        }
        float Dotproduct(VPoint a, VPoint b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }
    }
} // namespace Template