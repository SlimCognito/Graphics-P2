using System;
using System.IO;

namespace Template {         //het huidige probleem lijkt zich te bevinden in de sphere intersect, de rays intersecten nooit, en dat zou volgens mij wel moeten gebeuren. J.

    class Game
    {
        public bool      Debugging;
        public Raytracer Tracer;
	    public Surface   Screen;

	    // initialize
	    public void Init()
	    {
            // Add light(s)
            Light[] lights = new Light[1];
            lights[0] = new Light(new VPoint(1, 2, 0), 1, 1, 1);
            // Add primitive(s)
            Primitive[] primitives = new Primitive[4];
            primitives[0] = new Plane(new VPoint(0, 1, 0), -2, new Material(1f));
            primitives[1] = new Sphere(new VPoint(0, 0, 5), 1.5f, new Material(new VPoint(255, 50, 100), 0.5f));
            primitives[2] = new Sphere(new VPoint(-3, 0, 5), 1.5f, new Material(new VPoint(0, 255, 10), 0.5f));
            primitives[3] = new Sphere(new VPoint(3, 0, 5), 1.5f, new Material(new VPoint(255, 255, 255), 0.75f));
            // Create scene
            Scene scene = new Scene(lights, primitives);
            // Create raytracer
            Tracer = new Raytracer(scene, Screen);
            // Set debugging
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

    public class Material
    {
        public VPoint Color;
        public bool texture;
        public float Reflects;
        public VPoint GetColor(VPoint p)
        {
            if (!texture)
                return Color;
            else
            {
                return new VPoint(231, 231, 231) * ((((Math.Abs((int)Math.Floor(p.X) + (int)Math.Floor(p.Z)))) % 2) + 0.1f);
            }
        }
        // Create material
        public Material(float r)
        {
            texture = true;
            Reflects = r;
        }
        // Create reflective material
        public Material(VPoint c, float r)
        {
            Color = c;
            Reflects = r;
            texture = false;
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
        // 3D vector / point in 3D space
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

        public static VPoint colorStuff(VPoint reflectie, VPoint diffusie, float r)
        {
            VPoint a = diffusie * (1- r) + r * new VPoint((diffusie.X * reflectie.X) / 255, (diffusie.Y * reflectie.Y) / 255, (diffusie.Z * reflectie.Z) / 255);
            return a;
        }

        public int getColor()
        {
            int lekkah = (int)((int)X * 256 * 256 + (int)Y * 256 + (int)Z);
            string lekkahlekkah = lekkah.ToString("X");
            return Convert.ToInt32(lekkahlekkah, 16);
        }
    }

    // Location = ray origin, Direction = ray direction and Distance = ray length
    public struct Ray
    {
        public VPoint Location;
        public VPoint Direction;
        public float  Distance;
        public int recursion;

        public Ray(VPoint Locationinit, VPoint Directioninit)
        {
            Location = Locationinit;
            Direction = Directioninit.Normalize();
            Distance = float.PositiveInfinity;
            recursion = 0;
        }
        public void debug(Surface screen, VPoint endPoint)
        {
            int x1, x2;
            x1 = Location.transform("x");
            x2 = endPoint.transform("x");
            if (x1 <= 512 && x2 <= 512)
            {
                screen.Line(x1, Location.transform("y"), x2, endPoint.transform("y"), 0xFF0000);
            }
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
        public VPoint Target;
  
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

        public void moveCamera(VPoint direction)
        {
            Position += direction;
            Upperleft += direction;
            Upperright += direction;
            Lowerleft += direction;
            Lowerright += direction;
            Target += direction;
        }

        private void setXDirection()
        {
            float dX = Orientation.X;
            float dZ = -(dX * dX) / Orientation.Z;
            XDirection = new VPoint(dX, 0, dZ).Normalize();
        }
        private void setYDirection()
        {
            YDirection = XDirection % Orientation;
            if (YDirection.Y < 0)
                YDirection *= 1;
        }

        public void turnCamera(VPoint direction)
        {
            Orientation = (Target + direction - Position).Normalize();
            setXDirection();
            setYDirection();
            Upperleft = Target - XDirection - YDirection;
            Upperright = Target + XDirection - YDirection;
            Lowerleft = Target - XDirection + YDirection;
            Lowerright = Target + XDirection - YDirection;
        }

        public Ray getRay(float x,float y)
        {
            x /= 256;
            y /= 256;
            VPoint positionOnScreen = Upperleft;
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
            int x1, x2;
            x1 = Upperleft.transform("x");
            x2 = Upperright.transform("x");
            if (x1 <= 512 && x2 <= 512)
            {
                screen.Line(x1, Upperleft.transform("y"), x2, Upperright.transform("y"), 255255255);
            }
        }
    }
   
    abstract class Primitive
    {
        public Material Mat;
        abstract public Ray normal(VPoint location);
        abstract public void debug(Surface screen);
        abstract public float Intersect(Ray ray); // Misschien naar abstract public void Intersect en de intersection opslaan in class Intersect?

        public Ray Reflect(Ray ray, VPoint location)
        {
            VPoint d = ray.Direction.Normalize();
            VPoint n = normal(location).Direction;
            return new Ray(location, (d - (2 * (d * n) * n)).Normalize());
        }
    }

    // Radius2 = Radius^2, Location = centre of the sphere
    class Sphere : Primitive
    {
        public VPoint Location;
        public float  Radius;
        public float  Radius2;

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
            VPoint previousTekenpunt = new VPoint((float)Math.Sin(0), 0, (float)Math.Cos(0));
            previousTekenpunt = previousTekenpunt.Normalize()*Radius;
            previousTekenpunt += middle;

            for(double i = 1; i<121; i++)
            {
                double pii = i / 60 * Math.PI;
                VPoint tekenpunt = new VPoint((float)Math.Sin(pii), 0, (float)Math.Cos(pii));
                tekenpunt = tekenpunt.Normalize()*Radius;
                tekenpunt += middle;
                int x1, x2;
                x1 = tekenpunt.transform("x");
                x2 = previousTekenpunt.transform("x");
                if (x1 <= 512 && x2 <= 512)
                {
                    screen.Line(x1, tekenpunt.transform("y"), x2, previousTekenpunt.transform("y"), Mat.GetColor(new VPoint(0, 0, 0)).getColor());
                }
                previousTekenpunt = tekenpunt;
            }

        }
        // Intersects with a ray, returns the length at which the ray hits the sphere, -1 if no intersection
        override public float Intersect(Ray ray) 
        {
            VPoint c = Location - ray.Location;
            float t = c * ray.Direction;
            VPoint q = c - t * ray.Direction;
            float p = q * q;
            if (p > Radius2) return -1;
            t -= (float) Math.Sqrt(Radius2 - p);
            return t;
        }
        public override Ray normal(VPoint location) //Misschien aanpassen zodat we weten van binnen of van buiten -voorlopig niet zinnig omdat we maar vanuit een punt kijken en 
                                                    //dedichtstbijzijnde intersectie buiten hebben. J.
        {
            return new Ray(location, (location - Location).Normalize());
        }        
    }

    // Plane is determined by normal and distance to the origin.
    class Plane : Primitive
    {
        public VPoint Normal;
        public float  Distance;

        public Plane(VPoint normal, float distance, Material mat)
        {
            Mat = mat;
            Normal = normal.Normalize();
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
        public float  Red, Green, Blue;
        
        public Light(VPoint location, float r, float g, float b)
        {
            Location = location;
            Red = r;
            Green = g;
            Blue = b;
        }

        public VPoint reflectedColor(VPoint colorOfObject, float intensity)
        {
            return new VPoint((int)(colorOfObject.X * intensity*Red),(int)( colorOfObject.Y * intensity*Green), (int)(colorOfObject.Z * intensity*Blue));
        }
    }

    // A scene is a list of lights and primitives
    // The intersection methods checks for each ray whether it intersects with a primitive
    class Scene
    {
        public Light[]     Lights;
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
        public Ray       Ray;
        public VPoint    Location;
        public Primitive ThingWeIntersectedWith;
        public float     Distance;

        public Intersection(Ray ray,  VPoint location, Primitive p)
        {
            Distance = (location - ray.Location).Length;
            Ray = ray;
            Location = location;
            ThingWeIntersectedWith = p;
        }

        public VPoint color(Scene scene)
        {
            if (ThingWeIntersectedWith != null)
            {
                VPoint diffusion = new VPoint();
                foreach (Light light in scene.Lights)
                {
                    VPoint shadowRayDirection = (light.Location - Location);
                    Ray shadowRay = new Ray(Location + 0.00001f * shadowRayDirection.Normalize(), shadowRayDirection.Normalize());
                    float distance = scene.intersect(shadowRay).Distance;
                    if (distance >= shadowRayDirection.Length - 2 * 0.00001)
                    {
                        diffusion += light.reflectedColor(ThingWeIntersectedWith.Mat.GetColor(Location), 60*Math.Max(0, ThingWeIntersectedWith.normal(Location).Direction * shadowRay.Direction.Normalize()) * (1 / (shadowRayDirection.Length * shadowRayDirection.Length)));
                    }
                }
                diffusion = new VPoint(Math.Min(diffusion.X, 255), Math.Min(diffusion.Y, 255), Math.Min(diffusion.Z, 255));
                if (ThingWeIntersectedWith.Mat.Reflects != 0 && Ray.recursion < 5)
                {
                    Ray primaryRay = ThingWeIntersectedWith.Reflect(Ray, Location);
                    primaryRay.recursion = Ray.recursion + 1;

                    Intersection inter = scene.intersect(primaryRay);
                    VPoint smth = inter.color(scene);
                    return VPoint.colorStuff(inter.color(scene), diffusion, ThingWeIntersectedWith.Mat.Reflects);
                }
                else
                    return diffusion;
            }
            return new VPoint();
        }

        public void debug(Surface screen) 
        {
            Ray.debug(screen, Location);
            if(ThingWeIntersectedWith != null)
                ThingWeIntersectedWith.normal(Location).debug(screen, 1); 
            else
            {

            }
        }
    }

    class Raytracer
    {
        public Scene   Scene;
        public Camera  Camera;
        public Surface Screen;

        public Raytracer(Scene scene, Surface screen)
        {
            Scene = scene;
            Screen = screen;
            Camera = new Camera();
        }
        
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
                    if (debugging && y == 256 && x % 20 == 0)
                    {
                        if (intersection.ThingWeIntersectedWith != null)
                            intersection.debug(Screen);
                        else
                            ray.debug(Screen, 7);//smth
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