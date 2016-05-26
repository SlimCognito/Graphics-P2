using System;
using System.IO;

namespace Template {

    class Game
    {
	    // member variables 
	    public Surface screen;
	    // initialize
	    public void Init()
	    {
	    }
	    // tick: renders one frame
	    public void Tick()
	    {
		    screen.Clear( 0 );
		    screen.Print( "hello world", 2, 2, 0xffffff );
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
    }

    class Camera
    {
        public VPoint position = new VPoint(0, 0, 0);
        public VPoint orientation = new VPoint(0, 0, 1);
        public VPoint upperright = new VPoint(1, 1, 1);

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
        public static float operator *(VPoint id1, VPoint id2)
        {
            return (id1.x * id2.x + id1.y * id2.y + id1.z * id2.z);
        }
        public static VPoint operator *(VPoint id1, float id2)
        {
            return new VPoint(id1.x * id2, id1.y * id2, id1.z * id2);
        }
        public static VPoint operator *(float id1, VPoint id2)
        {
            return new VPoint(id1 * id2.x, id1 * id2.y, id1 * id2.z);
        }
        public static VPoint operator +(VPoint id1, VPoint id2)
        {
            return new VPoint(id1.x + id2.x, id1.y + id2.y, id1.z + id2.z);
        }
        public static VPoint operator -(VPoint id1, VPoint id2)
        {
            return new VPoint(id1.x - id2.x, id1.y - id2.y, id1.z - id2.z);
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
        public Sphere(VPoint Location1, float radius1)
        {
            Location = Location1;
            Radius = radius1;
            Radius2 = radius1 * radius1;
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
        public Plane(VPoint Normal1, float Distance1)
        {
            Normal = Normal1;
            Distance = Distance1;
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
        public float red, green, blue;
    }

    class Scene
    {
        public Light[] lights;
        public Primitive[] primitives;

        public Intersection intersect(Ray ray)
        {
            return null;
        }
    }

    class Intersection
    {
        public Ray normal;
        public VPoint location;
        public Primitive thingWeIntersectedWith;
    }

    class Raytracer
    {
        public Scene scene;
        public Camera camera;
        public Surface screen;

        public void Render()
        {
            for (int x = 0; x < screen.width; x++)
            {
                for (int y = 0; y < screen.height; y++)
                {
                    Ray ray = camera.getRay(x, y);
                    Intersection intersection = scene.intersect(ray);

                }
            }
        }
        float Dotproduct(VPoint a, VPoint b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }
    }
} // namespace Template