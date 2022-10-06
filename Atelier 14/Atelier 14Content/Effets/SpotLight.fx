
//--------------------------- EFFET SPOTLIGHT ----------------------------


//-------------------------------- SEMANTICS --------------------------------

// Semantic - g�n�rales
float4x4 Monde;
float4x4 MondeVueProjection;

// Semantic - Sources lumineuses
float3 PositionLumiere;
float RayonLumiere;

// Semantic - Mat�riau
float4 CouleurLumiereDiffuse;

// Semantic - Texture
bool TextureActive;  // Doit �tre � vrai (true) si l'on souhaite "colorer" les pixels � l'aide d'une texture. 
                     // Dans le cas contraire le shader utilise la couleur du param�tre CouleurLumi�reDiffuse.
texture Texture;


//-------------------------------- STRUCTURES --------------------------------

// D�claration de la structure du format de la texture, aucun filtre dans le cas pr�sent
sampler FormatTexture = sampler_state
{
	Texture = (Texture);
};

struct VertexShaderInput 
{
	float4 Position : POSITION0;            // Position du sommet dans l'espace 3D
	float3 Normale : NORMAL;                 // Normale du sommet dans l'espace 3D
	float2 CoordonneesTexture : TEXCOORD;   // Coordonn�es de texture (0..1, 0..1) li�es au sommet
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;              // Position du sommet en fonction de la matrice MondeVueProjection (Clip Space)	
	float2 CoordonneesTexture : TEXCOORD0;    // Coordonn�es de texture (0..1, 0..1) li�es au sommet
	float3 Normale : TEXCOORD1;               // Vecteur normal du pixel
	float Distance : FOG;					  // Distance entre le sommet et la lumi�re
};

VertexShaderOutput VertexShaderSpotLight(VertexShaderInput EntreeVS)
{
	VertexShaderOutput SortieVS;
	
	SortieVS.Distance = distance(PositionLumiere, mul(EntreeVS.Position, Monde));
	// Transformation des sommets en fonction de la matrice MondeVueProjection
	SortieVS.Position = mul(EntreeVS.Position, MondeVueProjection);
	// Affectation (sans transformation) des coordonn�es de texture qui seront interpol�es par le GPU
	SortieVS.CoordonneesTexture = EntreeVS.CoordonneesTexture;
 
	SortieVS.Normale = normalize(EntreeVS.Normale);

	return SortieVS;
}

float CalculerNorme(float3 vecteur)
{
	return sqrt(pow(vecteur.x, 2) + pow(vecteur.y, 2) + pow(vecteur.z, 2));
}

float4 PixelShaderSpotLight(VertexShaderOutput EntreePS) : COLOR0
{
	float4 couleurTexture;

	if (TextureActive)
	{
		couleurTexture = tex2D(FormatTexture, EntreePS.CoordonneesTexture);
	}
	else
	{
		couleurTexture = CouleurLumiereDiffuse;
	}

	if (EntreePS.Distance <= RayonLumiere)
	{
		couleurTexture *= (2.5f * CalculerNorme(EntreePS.Normale));
	}

	return couleurTexture;
}

technique Technique_SpotAVincent
{
    pass SpotLight
    {
        // On peut ajouter ici l'initialisation des Render State (RasterizerState, DepthStencilState, BlendState et autres)
		
		VertexShader = compile vs_3_0 VertexShaderSpotLight();
		PixelShader = compile ps_3_0 PixelShaderSpotLight();
    }
}
