## Terrain Generator


### Description
---
The Endless Terrain Generator is capable of creating realistic-looking terrains textured by elevation. It can create every terrain type, from oceans, lakes and, shallow waters to snowy mountain peaks. The generator can use terrain data and noise data templates to generate different kinds of biomes. The landmass can be created in the form of continents or islands. The purpose is to only generate landmass and terrain topography, and so the generator will not add vegetation. 

Water is added as a plain surface but the ground beneath is also generated with various shapes and depths.

### Technology
---
<dl>
  <dt>C#</dt>
  <dd>The C# scripts generate and interpret the Noise that is used to create and display the terrain.</dd>
  <dt>Materials</dt>
  <dd>Different materials have been created and templated so that the generator can display different-looking biomes.</dd>
  <dt>Unity</dt>
  <dd>The Unity game engine is used to display and vizualize the terrain data.
    The generated world can be explored in a first person view. </dd>
</dl>

### Functionality
---
The project contains a sample scene that showcases a model setup for the terrain generator.

A new object has to be added to the Scene. On the object, the Map Generator script and the Endless script have to be present. Each script needs a couple of correlations to work, such as the player object. 

The main necessities are the two storage formats added in the repository: Noise Storage and Terrain Storage. The data types can be created and configured to create custom biomes. A few examples are present in the project.


### Realeases
---
No realease is available yet.
