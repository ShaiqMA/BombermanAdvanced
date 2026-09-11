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

void main()
{
  // position on map in tiles
  vec2 mapLocation = mapOffset + mapScale * texCoord;
  
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
  
  // [RELEASE] output intended color
  if( isGreyScaleAlpha != 0 )
  {
    outputColor = vec4(1, 1, 1, texture(texture0, uv).r);
  }
  else
  {
    outputColor = texture(texture0, uv);
  }
  outputColor *= color;
  if( outputColor.a < 0.1 ) discard; // alpha test for transparency

  /*
  return;

  // [DEBUG] output localUv
  outputColor = vec4(localUv, 0.0, 1.0);
  return;

  // [DEBUG] output spriteIndex
  outputColor = vec4(spriteIndex/255.0, spriteIndex/255.0, spriteIndex/255.0, 1);
  return;

  // [DEBUG] output relative map location
  outputColor = vec4(mapLocation/mapSize, 0.0, 1.0);
  return;

  // [DEBUG] output texCoord
  outputColor = vec4(texCoord, 0.0, 1.0);
  return;

  // [DEBUG] output uv
  outputColor = vec4(uv, 0.0, 1.0);
  return;
  */
}