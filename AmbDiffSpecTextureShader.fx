
// Declare matrices and light direction
float4x4 World;
float4x4 View;
float4x4 Projection;
float4   LightDir;
float4   CamPos;
float4   LightColor; // General light color.
float4	 AmbientColor; // Ambient light color.
texture  ModelTexture;

sampler2D textureSampler = sampler_state {
    Texture = (ModelTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

// each input vertex contains a position, normal and texture
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0; 
};

// the values to be interpolated across triangle
struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TextureCoordinate : TEXCOORD0; 
	float4 WorldPosition : TEXCOORD1;
	float4 ViewPosition: TEXCOORD2;
    float4 Normal : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World); // object to world xform
    float4 viewPosition = mul(worldPosition, View);  // world to camera xform

    output.Position = mul(viewPosition, Projection); // perspective xform
	output.WorldPosition = worldPosition;
	output.ViewPosition = viewPosition;
	output.TextureCoordinate = input.TextureCoordinate;
	output.Normal = input.Normal;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float specularLight = 0.0f;
	float shininess = 80.0f;

	float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);  // get texture color
	textureColor.a = 1.0;

	// Calculate direction of camera from vertex position.
	float4 viewDir = normalize(CamPos - input.WorldPosition);

	// Cacluate diffuse directional lighting.
	float diffuseLight = saturate(dot(input.Normal, LightDir));

	// Calculate light reflection from vertex and light source.
	float4 reflectedLight = (2 * dot(input.Normal, LightDir) * input.Normal) - LightDir;

	// Calculate specular lighting.
	specularLight = pow(saturate(dot(reflectedLight, viewDir)), shininess);

	// Calculate final color by adding all light effects together.
	float4 finalColor = (specularLight * LightColor) + 
	(textureColor * diffuseLight) + (textureColor * AmbientColor);
	finalColor.a = 0.4f;

	//finalColor = 0.01f * finalColor + input.Normal; // Shows surface normal.
	//finalColor = 0.01f * finalColor + specularLight * LightColor; // Shows specular lighting.

	return saturate(finalColor);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
