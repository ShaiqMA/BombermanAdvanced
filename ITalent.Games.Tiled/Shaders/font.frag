#version 330

out vec4 outputColor;

in vec2 texCoord;

uniform ivec2 mapSize;
uniform ivec2 sheetSize;
uniform vec2 mapOffset;
uniform vec2 mapScale;

// A sampler2d is the representation of a texture in a shader.
// Each sampler is bound to a texture unit (texture units are described in Texture.cs on the Use function).
// By default, the unit is 0, so no code-related setup is actually needed.
// Multiple samplers will be demonstrated in section 1.5.

// sprite sheet
uniform sampler2D texture0;

// tile map (game world)
uniform sampler2D texture1;

uniform int isGreyScaleAlpha;
uniform vec4 color = vec4(1);
 
//bias: 0.17353355999581582 ( very probably the best of its kind )
uint lowbias32(uint x)
{
    x ^= x >> 16;
    x *= 0x7feb352dU;
    x ^= x >> 15;
    x *= 0x846ca68bU;
    x ^= x >> 16;
    return x;
}

float sample(vec2 mapLocation)
{ 
  if( mapLocation.x < 0.0 ) discard;
  if( mapLocation.y < 0.0 ) discard;
  if( mapLocation.x > mapSize.x ) discard;
  if( mapLocation.y > mapSize.y ) discard;
  
  // tile 0..255 at position. assumes nearest pixel sampling (no interpolation)
  int spriteIndex = int(texture(texture1, mapLocation/mapSize).r * 255.0);

  // local uv within that tile
  vec2 localUv = fract(mapLocation);

  // sprite coords
  int spriteX = spriteIndex % sheetSize.x;
  int spriteY = int(spriteIndex / sheetSize.x);

  // tex coords within sprite sheet
  vec2 uv = vec2(
    (spriteX + localUv.x) / sheetSize.x,
    (spriteY + localUv.y) / sheetSize.y
  );
  
  return texture(texture0, uv).r;
}

void main()
{
  float alpha = sample(mapOffset + mapScale * texCoord);
  if( alpha < 0.1 )  
  {
    // each tile is 9x16 pixels, which is why the offset is 1/9, 1/16, aka: 1 pixel
    alpha = sample(mapOffset + vec2(-1.0/9, 1.0/16) + mapScale * texCoord);
    if( alpha < 0.1 )
    {
      discard;
    }
    else
    {
      outputColor = vec4(0, 0, 0, 1);
    }
  }
  else
  {
    outputColor = color;
  }
}