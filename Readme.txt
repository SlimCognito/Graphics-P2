David van Balen 5513588
Joris van Gool  4270126
Paul van Grol   5528909

Implemented bonus assignments
 Skydome- space, because that seemed to fit best. This is of course also a texture implementation on a sphere containing a real image(or something that looks like it, a jpeg in any case)
 This required us to find an implementation to find intersections of a ray and a sphere where the ray's origin is withing the sphere.
 The implementation of the intersection can be found in Sphere, the skydome itself is a Sphere with a rather large radius and a specific texture (the Space.jpg)

Materials used to implement the ray tracer
 Pseudocode from the slides for:
  Ray - Sphere intersection
  Normal on sphere
  Reflection on sphere and plane
 Space.jpg is a random picture from the web
 some bitmap tricks from stack overflow&C# database

It should be noted that for ease with debugging the locations and sizes of the spheres have been fixed. When moving the camera
the intersection points move as they should, the spheres themselves don't.
Another thing to note is that the tracer is incredibly slow, moving the camera especially goes slow and requires you either constantly hitting
or constantly pressing the button mapped to the movement you wanna make
A: Turn left
D: Turn right
W: Turn up
S: Turn down
Enter: Move up
LShift: Move down
Up: Move forward
Down: Move backwards
Left: Move left
Right: Move right
Using Up and Down you can change the FOV