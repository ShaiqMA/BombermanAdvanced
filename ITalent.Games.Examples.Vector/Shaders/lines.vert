#version 330 core

layout(location = 0) in vec2 aPosition;

void main(void)
{
    gl_Position = vec4(aPosition, 1.0, 1.0);
}