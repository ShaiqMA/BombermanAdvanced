#version 330 core
layout (location = 0) in vec2 aPos;       // position of vertex in mesh
layout (location = 1) in vec2 aTexCoord;  // tex coord of vertex in mesh
layout (location = 2) in vec2 aOffset;    // instanced parameter: position of sprite in world
layout (location = 3) in float aIndex;    // instanced parameter: index of sprite within the sprite sheet
layout (location = 4) in float aScale;    // instanced parameter: uniform scale of sprite
layout (location = 5) in float aFlip;    // instanced parameter: flip bits: 1 = H, 2 = V, 3 = rot90

out vec2 texCoord;

uniform ivec2 sheetSize;
uniform vec2 mapOffset;
uniform vec2 mapScale;


//bias: 0.17353355999581582 ( very probably the best of its kind )
int lowbias32(int x)
{
    x ^= x >> 16;
    x *= 0x7feb352d;
    x ^= x >> 15;
    x *= 0x846ca68b;
    x ^= x >> 16;
    return x;
}


void main()
{
    texCoord = aTexCoord;
    
    // temp
    int spriteIndex = int(aIndex); //lowbias32(gl_InstanceID) % (sheetSize.x * sheetSize.y);
    int spriteFlip = int(aFlip);
    
    // sprite coords
    int spriteX = spriteIndex % sheetSize.x;
    int spriteY = int(spriteIndex / sheetSize.x);

    vec2 localUv = aTexCoord;
    if( (spriteFlip & 1) == 1 ) { localUv.x = 1.0 - localUv.x; }
    if( (spriteFlip & 2) == 2 ) { localUv.y = 1.0 - localUv.y; }
    if( (spriteFlip & 4) == 4 ) {
      // rotate 90CW
      float temp = localUv.y;
      localUv.y = 1.0 - localUv.x;
      localUv.x = temp;
    }

    // tex coords within sprite sheet
    texCoord.x = (spriteX + localUv.x) / sheetSize.x;
    texCoord.y = (spriteY + localUv.y) / sheetSize.y;
    
    // play around with scale using gl_InstanceID
    //vec2 pos = aPos * (gl_InstanceID / 100.0);

    // aPos : positie binnen Quad mesh 0 ... 1
    // aOffset : positie van individuele sprite, gemeten in tiles
    // mapScale : scale van map tiles naar sprite space, scale (1,1) betekent: een sprite neemt het hele beeld in beslag
    vec2 scaledPos = aScale * aPos;
    gl_Position = vec4(mapOffset + mapScale * (scaledPos + 2 * aOffset), 0.0, 1.0);
}  