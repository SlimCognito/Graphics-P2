using System;
using System.IO;

namespace Template {         //het huidige probleem lijkt zich te bevinden in de sphere intersect, de rays intersecten nooit, en dat zou volgens mij wel moeten gebeuren. J.

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
            primitives[0] = new Plane(new VPoint(0, 1, 0), -5, 255000);
            primitives[1] = new Sphere(new VPoint(0, 0, 5), 1, 255000);
            primitives[2] = new Sphere(new VPoint(-3, 0, 5), 1, 255255000);
            primitives[3] = new Sphere(new VPoint(3, 0, 5), 1, 255000000);
            //voeg de primitives toe
            Scene scene = new Scene(lights, primitives);
            Tracer = new Raytracer(scene, Screen);
            Debugging = true;
	    }
	    // tick: renders one frame
	    public void Tick()
	    {
		    Screen.Clear( 0 );
		    Screen.Print( "Ray Tracer", 2, 2, 0xffffff );
            Tracer.Render(Debugging);
	    }
    }

    // X,Y,Z = coordinates, RememberLength = used to determine whether the lenght has been calculated yet. If not the length will be calculated.
    // RememberLength has been implemented with the idea that we won't always need the length of the vector.
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

        // Used in debugging window, Y = 0 so doesn't have to be transformed.
        public int transform(string coordinate)
        {
            if (coordinate == "x")
            {
                return (int)((5 + X) * 51.2f);
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
    }

    // Location = ray origin, Direction = ray direction and Distance = ray length
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

        public Camera()
        {
            Position = new VPoint(0, 0, 0);
            Orientation = new VPoint(0, 0, 1);
            Upperleft = new VPoint(-1, 1, 1);
            XDirection = new VPoint(1, 0, 0);
            YDirection = new VPoint(0, -1, 0);
            Upperright = new VPoint(1, 1, 1);
            Lowerleft = new VPoint(-1, -1, 1);
            Lowerright = new VPoint(1, -1, 1);
        }

        public void moveCamera(VPoint direction)
        {
            Position += direction;
            Upperleft += direction;
            Upperright += direction;
            Lowerleft += direction;
            Lowerright += direction;
        }

        public void turnCamera(VPoint direction)
        {
            VPoint target = Position + Orientation;
            target += direction;

        }

        public Ray getRay(float x,float y)
        {
            x /= 256;
            y /= 256;
            VPoint positionOnScreen = new VPoint(Upperleft.X, Upperleft.Y, Upperleft.Z);
            positionOnScreen += x * XDirection + y * YDirection;
            return new Ray(Position, (positionOnScreen - Position).Normalize());
        }

        public void Update()
        {
            /*Upperleft = Upperleft;
            Upperright = Upperright;
            Lowerleft = Lowerleft;
            Lowerright = Lowerright;*/
        }

        public void debug(Surface screen)
        {
            screen.Line(Upperleft.transform("x"), Upperleft.transform("y"), Upperright.transform("x"), Upperright.transform("y"), 255255255);
        }
    }
   
    abstract class Primitive
    {
        public int Color = 1;
        abstract public Ray normal(VPoint location);
        abstract public void debug(Surface screen);
        abstract public float Intersect(Ray ray); // Misschien naar abstract public void Intersect en de intersection opslaan in class Intersect?
    }

    // Radius2 = Radius^2, Location = centre of the sphere
    class Sphere : Primitive
    {
        public VPoint Location;
        public float Radius;
        public float Radius2;
        public Sphere(VPoint location, float radius, int color)
        {
            Color = color;
            Location = location;
            Radius = radius;
            Radius2 = radius * radius;
        }
        public override void debug(Surface screen)
        {
            float newradius = (float)Math.Sqrt(Radius2 - Location.Y);
            VPoint middle = Location;
            middle.Y = 0;
            VPoint previousTekenpunt = new VPoint((float)Math.Sin(0), 0, (float)Math.Cos(0));
            previousTekenpunt += middle;
            for(int i = 1; i<120; i++)
            {
               double pii = i / 60 * Math.PI;
               VPoint tekenpunt = new VPoint((float)Math.Sin(i), 0, (float)Math.Cos(i));
               tekenpunt += middle;
               screen.Line(tekenpunt.transform("x"), tekenpunt.transform("y"), previousTekenpunt.transform("x"), previousTekenpunt.transform("y"), Color);
            }
        }
        // Intersects with a ray, returns the length at which the ray hits the sphere, -1 if no intersection
        override public float Intersect(Ray ray) 
        {
            VPoint c = Location - ray.Location;
            float t = c * ray.Direction;
            if (t < 0) return -1;
            float d = t * t - c * c;
            if (d > Radius2) return -1;
            float tc = (float)Math.Sqrt(Radius2 - d);
            float intersect = t - tc;
            return intersect;
            VPoint q = c - t * ray.Direction;
            float p = q * q;
            if (p > Radius * Radius2) return -1;
            t -= (float) Math.Sqrt(Radius2 - p);
            ray.Distance = Math.Min(ray.Distance, Math.Max(0, t));
            return ray.Distance;
        }
        public override Ray normal(VPoint location) //Misschien aanpassen zodat we weten van binnen of van buiten -voorlopig niet zinnig omdat we maar vanuit een punt kijken en 
                                                    //dedichtstbijzijnde intersectie buiten hebben. J.
        {
            VPoint centre = Location;
            throw new NotImplementedException();
        }
    }

    // Plane is determined by normal and distance to the origin.
    class Plane : Primitive
    {
        public VPoint Normal;
        public float Distance;
        public Plane(VPoint normal, float distance, int color)
        {
            Color = color;
            Normal = normal;
            Distance = distance;
        }
        public override void debug(Surface screen)
        {
            //hier doen we voorlopig niks  omdat het niet zinnig is.
        }
        override public float Intersect(Ray ray)
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

    // Location = location of the light source, Red, Green and Blue determine the light intensity
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

    // A scene is a list of lights and primitives
    // The intersection methods checks for each ray whether it intersects with a primitive
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
                if(j< ray.Distance&&j>0)
                {
                    ray.Distance = j;
                    Hit = p;
                }
            }
            return new Intersection(ray, ray.Location + ray.Direction*ray.Distance, Hit);
        }
    }

    class Intersection // Intersections opslaan in een linkedlist en vervolgens de lijst langslopen? wat voegt dit toe? we hebben toch maar een intersetion, de dichtbijste? J.
    {
        public LinkedList Intersections = new LinkedList();
        public Ray Ray;
        public VPoint Location;
        public Primitive ThingWeIntersectedWith;

        public Intersection(Ray ray,  VPoint Location, Primitive p)
        {
            this.Ray = ray;
            this.Location = Location;
            this.ThingWeIntersectedWith = p;
        }

        public void debug(Surface screen) //ik mis even wat hier gebeurt J.
        {
            Ray.debug(screen, Location);
            if(ThingWeIntersectedWith != null)
                //ThingWeIntersectedWith.normal(Location).debug(screen, 1); 
            //else
            {

            }
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
        
        public void Render(bool debugging) // tijdelijk y standaard op 0 gezet voor EZ debugging J.
        {
            Ray ray; int y = 0;
            for (int x = 0; x < Screen.width; x++)
            {
                //for (int y = 0; y < Screen.height; y++)
                //{
                    ray = Camera.getRay(x, y);
                    Intersection intersection = Scene.intersect(ray);

                    if (debugging && y == 0 && x % 20 == 0)
                    {
                        if (intersection.ThingWeIntersectedWith != null)
                            intersection.debug(Screen);
                        else
                            ray.debug(Screen, 8);//smth
                    }

               // }
            }
            if (debugging)
            {
                foreach (Primitive p in Scene.Primitives)
                    //p.debug();
                Camera.debug(Screen);
            }
        }
    }

    /*
     * public void drawpixel(int x, int y, Primitive p)
     * {
     * screen.Line(x,y,x,y, p.Color)
     * }
     */


    class Application
    {
        
    }

    public class LinkedList
    {
        public class Node
        {
            public Node next = null;
            public object data;
        }

        private Node root = null;
        
        public Node First { get { return root; } }

        public Node Last
        {
            get
            {
                Node current = root;
                if (current == null)
                    return null;
                while (current.next != null)
                    current = current.next;
                return current;
            }
        }

        public void Add(object value)
        {
            Node n = new Node { data = value };
            if (root == null)
                root = n;
            else
                Last.next = n;
        }

        public void Delete(Node n)
        {
            if (root == n)
            {
                root = n.next;
                n.next = null;
            }
            else
            {
                Node current = root;
                while (root.next != null)
                {
                    if (current.next == n)
                    {
                        current.next = n.next;
                        n.next = null;
                        break;
                    }
                    current = current.next;
                }
            }
        }
    }
}