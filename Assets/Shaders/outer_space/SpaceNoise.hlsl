#ifndef SPACE_NOISE_INCLUDED
#define SPACE_NOISE_INCLUDED

// Self-contained noise helpers for the deep-space skybox.
// (Prefixed "Space" so it never clashes with other shader packs.)

float SpaceHash11(float n)
{
    return frac(sin(n * 127.1) * 43758.5453123);
}

// Stable hash (Dave Hoskins), returns 0..1
float3 SpaceHash33(float3 p)
{
    p = frac(p * float3(0.1031, 0.1030, 0.0973));
    p += dot(p, p.yxz + 33.33);
    return frac((p.xxy + p.yxx) * p.zyx);
}

// Quintic gradient noise, roughly -0.8..0.8
float SpaceGradNoise(float3 p)
{
    float3 i = floor(p);
    float3 f = frac(p);
    float3 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);

    float n000 = dot(SpaceHash33(i + float3(0, 0, 0)) * 2.0 - 1.0, f - float3(0, 0, 0));
    float n100 = dot(SpaceHash33(i + float3(1, 0, 0)) * 2.0 - 1.0, f - float3(1, 0, 0));
    float n010 = dot(SpaceHash33(i + float3(0, 1, 0)) * 2.0 - 1.0, f - float3(0, 1, 0));
    float n110 = dot(SpaceHash33(i + float3(1, 1, 0)) * 2.0 - 1.0, f - float3(1, 1, 0));
    float n001 = dot(SpaceHash33(i + float3(0, 0, 1)) * 2.0 - 1.0, f - float3(0, 0, 1));
    float n101 = dot(SpaceHash33(i + float3(1, 0, 1)) * 2.0 - 1.0, f - float3(1, 0, 1));
    float n011 = dot(SpaceHash33(i + float3(0, 1, 1)) * 2.0 - 1.0, f - float3(0, 1, 1));
    float n111 = dot(SpaceHash33(i + float3(1, 1, 1)) * 2.0 - 1.0, f - float3(1, 1, 1));

    float x00 = lerp(n000, n100, u.x);
    float x10 = lerp(n010, n110, u.x);
    float x01 = lerp(n001, n101, u.x);
    float x11 = lerp(n011, n111, u.x);
    return lerp(lerp(x00, x10, u.y), lerp(x01, x11, u.y), u.z);
}

// Fractal Brownian motion with rotated octaves, approx -1..1
float SpaceFbm(float3 p, int octaves)
{
    const float3x3 m = float3x3( 0.00,  0.80,  0.60,
                                -0.80,  0.36, -0.48,
                                -0.60, -0.48,  0.64);
    float sum = 0.0;
    float amp = 0.5;
    [loop]
    for (int i = 0; i < octaves; i++)
    {
        sum += amp * SpaceGradNoise(p);
        p = mul(m, p) * 2.02;
        amp *= 0.5;
    }
    return sum * 1.3;
}

#endif // SPACE_NOISE_INCLUDED
