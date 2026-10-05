#ifndef EARTH_COMMON_INCLUDED
#define EARTH_COMMON_INCLUDED

// Shared helpers for the Earth shader pack.
// IMPORTANT: include this AFTER URP's Core.hlsl (it uses _MainLightPosition).

// Set every frame by EarthSystem.cs. If the script is missing, we fall back to
// URP's main directional light and a default sunlight colour.
float4 _EarthSunPos;    // xyz = Sun world position, w = 1 when valid
float4 _EarthSunColor;  // rgb = colour * intensity, w = 1 when valid

float3 EarthSunDir(float3 posWS)
{
    if (_EarthSunPos.w > 0.5)
        return normalize(_EarthSunPos.xyz - posWS);
    return normalize(_MainLightPosition.xyz);
}

float3 EarthSunColor()
{
    return (_EarthSunColor.w > 0.5) ? _EarthSunColor.rgb : float3(1.5, 1.45, 1.35);
}

// ---------------------------------------------------------------------------
// Noise (no textures, no UV seams)
// ---------------------------------------------------------------------------
float3 EarthHash33(float3 p)
{
    p = frac(p * float3(0.1031, 0.1030, 0.0973));
    p += dot(p, p.yxz + 33.33);
    return frac((p.xxy + p.yxx) * p.zyx);
}

float EarthGradNoise(float3 p)
{
    float3 i = floor(p);
    float3 f = frac(p);
    float3 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);

    float n000 = dot(EarthHash33(i + float3(0, 0, 0)) * 2.0 - 1.0, f - float3(0, 0, 0));
    float n100 = dot(EarthHash33(i + float3(1, 0, 0)) * 2.0 - 1.0, f - float3(1, 0, 0));
    float n010 = dot(EarthHash33(i + float3(0, 1, 0)) * 2.0 - 1.0, f - float3(0, 1, 0));
    float n110 = dot(EarthHash33(i + float3(1, 1, 0)) * 2.0 - 1.0, f - float3(1, 1, 0));
    float n001 = dot(EarthHash33(i + float3(0, 0, 1)) * 2.0 - 1.0, f - float3(0, 0, 1));
    float n101 = dot(EarthHash33(i + float3(1, 0, 1)) * 2.0 - 1.0, f - float3(1, 0, 1));
    float n011 = dot(EarthHash33(i + float3(0, 1, 1)) * 2.0 - 1.0, f - float3(0, 1, 1));
    float n111 = dot(EarthHash33(i + float3(1, 1, 1)) * 2.0 - 1.0, f - float3(1, 1, 1));

    float x00 = lerp(n000, n100, u.x);
    float x10 = lerp(n010, n110, u.x);
    float x01 = lerp(n001, n101, u.x);
    float x11 = lerp(n011, n111, u.x);
    return lerp(lerp(x00, x10, u.y), lerp(x01, x11, u.y), u.z);
}

float EarthFbm(float3 p, int octaves)
{
    const float3x3 m = float3x3( 0.00,  0.80,  0.60,
                                -0.80,  0.36, -0.48,
                                -0.60, -0.48,  0.64);
    float sum = 0.0;
    float amp = 0.5;
    [loop]
    for (int i = 0; i < octaves; i++)
    {
        sum += amp * EarthGradNoise(p);
        p = mul(m, p) * 2.02;
        amp *= 0.5;
    }
    return sum * 1.3;
}

// ---------------------------------------------------------------------------
// Procedural clouds. Used by BOTH the cloud shell and the ground (for cloud shadows),
// so the two always agree.
// ---------------------------------------------------------------------------
struct EarthCloudParams
{
    float scale;
    float coverage;
    float sharp;
    float wind;     // rad/sec drift relative to the ground
    float shear;
    float boil;     // how fast cloud shapes evolve
    float bands;    // latitude banding strength
    float seed;
};

float EarthBand(float x, float centre, float width)
{
    float d = (x - centre) / width;
    return exp(-d * d);
}

// dir = unit direction from the planet centre (object space). Returns cloud density 0..1.
float EarthCloudDensity(float3 dir, float t, EarthCloudParams P, int octaves)
{
    float lat = dir.y;

    // Steady zonal drift plus a bounded wobble (so patterns never smear into stripes over time)
    float a = t * P.wind + P.shear * 0.25 * sin(lat * 4.0 + t * 0.02);
    float s, c;
    sincos(a, s, c);
    float3 p = float3(c * dir.x - s * dir.z, dir.y, s * dir.x + c * dir.z);

    // Domain-warped fbm gives swirling, organic cloud shapes
    float3 q = p * P.scale + P.seed;
    float w = EarthFbm(q * 0.6 + float3(0.0, 0.0, t * P.boil), 3);
    float n = EarthFbm(q + w * 1.1 + float3(0.0, t * P.boil * 0.5, 0.0), octaves) * 0.5 + 0.5;

    // Earth-like climate belts: cloudy ITCZ at the equator, clear subtropics (~25 deg),
    // stormy mid-latitudes, cloudy poles
    float al = abs(lat);
    float band = 0.55 * EarthBand(al, 0.00, 0.12)
               + 0.45 * EarthBand(al, 0.62, 0.18)
               + 0.30 * EarthBand(al, 0.95, 0.15)
               - 0.25 * EarthBand(al, 0.30, 0.10);
    n += (band - 0.35) * P.bands * 0.5;

    float th = 1.0 - P.coverage;
    return smoothstep(th - P.sharp, th + P.sharp, n);
}

#endif // EARTH_COMMON_INCLUDED
