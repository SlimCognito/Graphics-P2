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
        public VPoint Direction;
        public VPoint Distance;
        public VPoint Location;
        public Ray(VPoint Directioninit, VPoint Locationinit, VPoint DistanceInit)
        {
            Direction = Directioninit;
            Distance = DistanceInit;
            Location = Locationinit;
        }
    }

    class Camera
    {
        public VPoint position = new VPoint(0, 0, 0);
        public VPoint orientation = new VPoint(0, 0, 1);
        public VPoint upperright = new VPoint(1, 1, 1);
    }

    public struct VPoint
    {
        public float x;
        public float y;
        public float z;
        public float lenght;

        public VPoint(float xinit, float yinit, float zinit)
        {
            x = xinit;
            y = yinit;
            z = zinit;
            lenght = (float)Math.Sqrt(x* x + y * y + z* z);
        }
        public VPoint Normalize()
        {
            return new VPoint(x/lenght, y/lenght, z/lenght);
        }
        public static VPoint operator *(VPoint id1, VPoint id2)     //ik heb issies met het niet normaal kunnen gebruiken van opperators.
        {
            return new VPoint(id1.x * id2.x, id1.y * id2.y, id1.z * id2.z);
        }
        public static VPoint operator *(VPoint id1, float id2)
        {
            return new VPoint(id1.x * id2, id1.y * id2, id1.z * id2);
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
        abstract public float Intersect
        {

        }
    }

    class Sphere : Primitive
    {
        public VPoint Location;
        public float radius;
        public Sphere(VPoint Location1, float radius1)
        {
            Location = Location1;
            radius = radius1;
        }
        override public float Intersect(Ray ray) 
            //tot zover directe copypasta van "goede manier om spheres the intersecten van de slides. iemand moet dit even uitdiepen en betere namen geven. :D
        {
            VPoint c = Location - ray.Location;
            float t = (c * ray.Direction).lenght;
            VPoint q = c - (ray.Direction * t);
            float p2 = (q * q).lenght;

            return 0;
        }
    }

    class Plane : Primitive
    {
        public VPoint Normal;
        public float Distance;
        public Plane(VPoint Normal1, int Distance1)
        {
            Normal = Normal1;
            Distance = Distance1;
        }
        override public float Intersect(Ray ray)  //volgens mij werkt dit maar ik zou er gelukkig van worden als iemand dit checkt.
        {
            VPoint intersection = new VPoint(0, 0, 0);
            VPoint partialVector = ray.Direction * Normal;
            if (partialVector.lenght == 0 )
            {
                return intersection.lenght;
            }
            intersection = ray.Direction * (Distance / partialVector.lenght);
            intersection -= ray.Location * Normal;
            return intersection.lenght;
        }
    }
} // namespace Template