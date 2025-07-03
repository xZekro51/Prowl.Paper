#version 450

layout(location = 0) in vec2 fsin_TexCoord;
layout(location = 1) in vec4 fsin_Color;
layout(location = 2) in vec2 fsin_Position;

layout(location = 0) out vec4 fsout_Color;

layout(set = 1, binding = 0) uniform texture2D Texture;
layout(set = 1, binding = 1) uniform sampler Sampler;

// Scissoring uniforms
layout(set = 2, binding = 0) uniform ScissorBuffer {
    mat4 ScissorMat;
    vec2 ScissorExt;
};

// Brush uniforms
layout(set = 3, binding = 0) uniform BrushBuffer {
    mat4 BrushMat;
    vec4 BrushColor1;    // Start color
    vec4 BrushColor2;    // End color
    vec4 BrushParams;    // x,y = start point, z,w = end point (or center+radius for radial)
    vec2 BrushParams2;   // x = Box radius, y = Box Feather
    int BrushType;       // 0=none, 1=linear, 2=radial, 3=box
};

float calculateBrushFactor() {
    // No brush
    if (BrushType == 0) return 0.0;
    
    vec2 transformedPoint = (BrushMat * vec4(fsin_Position, 0.0, 1.0)).xy;

    // Linear brush
    if (BrushType == 1) {
        vec2 startPoint = BrushParams.xy;
        vec2 endPoint = BrushParams.zw;
        vec2 line = endPoint - startPoint;
        float lineLength = length(line);
        
        if (lineLength < 0.001) return 0.0;
        
        vec2 posToStart = transformedPoint - startPoint;
        float projection = dot(posToStart, line) / (lineLength * lineLength);
        return clamp(projection, 0.0, 1.0);
    }
    
    // Radial brush
    if (BrushType == 2) {
        vec2 center = BrushParams.xy;
        float innerRadius = BrushParams.z;
        float outerRadius = BrushParams.w;
        
        if (outerRadius < 0.001) return 0.0;
        
        float distance = smoothstep(innerRadius, outerRadius, length(transformedPoint - center));
        return clamp(distance, 0.0, 1.0);
    }
    
    // Box brush
    if (BrushType == 3) {
        vec2 center = BrushParams.xy;
        vec2 halfSize = BrushParams.zw;
        float radius = BrushParams.x;
        float feather = BrushParams.y;
        
        if (halfSize.x < 0.001 || halfSize.y < 0.001) return 0.0;
        
        vec2 q = abs(transformedPoint - center) - (halfSize - vec2(radius));
        float dist = min(max(q.x,q.y),0.0) + length(max(q,0.0)) - radius;
        return clamp((dist + feather * 0.5) / feather, 0.0, 1.0);
    }
    
    return 0.0;
}

// Determines whether a point is within the scissor region and returns the appropriate mask value
// p: The point to test against the scissor region
// Returns: 1.0 for points fully inside, 0.0 for points fully outside, and a gradient for edge transition
float scissorMask(vec2 p) {
    // Early exit if scissoring is disabled (when scissorExt.x is negative or zero)
    if(ScissorExt.x <= 0.0) return 1.0;
    
    // Transform point to scissor space
    vec2 transformedPoint = (ScissorMat * vec4(p, 0.0, 1.0)).xy;
    
    // Calculate signed distance from scissor edges (negative inside, positive outside)
    vec2 distanceFromEdges = abs(transformedPoint) - ScissorExt;
    
    // Apply offset for smooth edge transition (0.5 creates half-pixel anti-aliased edges)
    vec2 smoothEdges = vec2(0.5, 0.5) - distanceFromEdges;
    
    // Clamp each component and multiply to get final mask value
    // Result is 1.0 inside, 0.0 outside, with smooth transition at edges
    return clamp(smoothEdges.x, 0.0, 1.0) * clamp(smoothEdges.y, 0.0, 1.0);
}


// Determines whether a point is within the scissor region and returns the appropriate mask value
// p: The point to test against the scissor region
// Returns: 1.0 for points fully inside, 0.0 for points fully outside, and a gradient for edge transition
float scissorMaskA(vec2 p) {
    // Early exit if scissoring is disabled (when scissorExt.x is negative or zero)
    if(ScissorExt.x <= 0.0) return 0.5;
    
    // Transform point to scissor space
    vec2 transformedPoint = (vec4(p, 0.0, 1.0)).xy;
    
    // Calculate signed distance from scissor edges (negative inside, positive outside)
    vec2 distanceFromEdges = abs(transformedPoint) - ScissorExt;
    
    // Apply offset for smooth edge transition (0.5 creates half-pixel anti-aliased edges)
    vec2 smoothEdges = vec2(0.5, 0.5) - distanceFromEdges;
    
    // Clamp each component and multiply to get final mask value
    // Result is 1.0 inside, 0.0 outside, with smooth transition at edges
    return clamp(smoothEdges.x, 0.0, 1.0) * clamp(smoothEdges.y, 0.0, 1.0);
}

float scissorMaskB(vec2 p) {
    //return 1.0;
    // Early exit if scissoring is disabled (when scissorExt.x is negative or zero)
    if(ScissorExt.x <= 0.0) return 1.0;
    
    // Transform point to scissor space
    vec2 transformedPoint = (ScissorMat * vec4(p, 0.0, 1.0)).xy;
    
    // Calculate signed distance from scissor edges (negative inside, positive outside)
    vec2 distanceFromEdges = abs(transformedPoint) - ScissorExt;
    
    // Apply offset for smooth edge transition (0.5 creates half-pixel anti-aliased edges)
    vec2 smoothEdges = vec2(0.5, 0.5) - distanceFromEdges;
    
    // Clamp each component and multiply to get final mask value
    // Result is 1.0 inside, 0.0 outside, with smooth transition at edges
    return clamp(smoothEdges.x, 0.0, 1.0) * clamp(smoothEdges.y, 0.0, 1.0);
}

float scissorMaskC(vec2 p) {
    // Early exit if scissoring is disabled (when scissorExt.x is negative or zero)
    if(ScissorExt.x <= 0.0) return 1.0;
    
    // Transform point to scissor space
    vec2 transformedPoint = (ScissorMat * vec4(p, 0.0, 1.0)).xy;
    // Work in screen space directly
    vec2 distanceFromEdges = max(vec2(0.0) - transformedPoint,  // Distance from min (0,0)
                                transformedPoint - ScissorExt);   // Distance from max (width,height)
    
    // Apply offset for smooth edge transition (0.5 creates half-pixel anti-aliased edges)
    vec2 smoothEdges = vec2(0.5, 0.5) - distanceFromEdges;
    
    // Clamp each component and multiply to get final mask value
    // Result is 1.0 inside, 0.0 outside, with smooth transition at edges
    return clamp(smoothEdges.x, 0.0, 1.0) * clamp(smoothEdges.y, 0.0, 1.0);
}

void main_test()
{
    fsout_Color = fsin_Color;
}

void main()
{
    vec2 pixelSize = fwidth(fsin_TexCoord);
    vec2 edgeDistance = min(fsin_TexCoord, 1.0 - fsin_TexCoord);
    float edgeAlpha = smoothstep(0.0, pixelSize.x, edgeDistance.x) * smoothstep(0.0, pixelSize.y, edgeDistance.y);
    edgeAlpha = clamp(edgeAlpha, 0.0, 1.0);
    
    float mask = scissorMask(fsin_Position);
    vec4 color = fsin_Color;

    // Apply brush if active
    if (BrushType > 0) {
        float factor = calculateBrushFactor();
        color = mix(BrushColor1, BrushColor2, factor);
    }
    
    vec4 texColor = texture(sampler2D(Texture, Sampler), fsin_TexCoord);
    color *= texColor;
    color *= edgeAlpha * mask;
    fsout_Color = color;
    //fsout_Color = vec4(color.a,color.a,color.a,255);
    //fsout_Color = vec4(mask,mask,mask,1);
}