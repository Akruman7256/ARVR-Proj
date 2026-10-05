#ifndef SUN_NOISE_INCLUDED
#define SUN_NOISE_INCLUDED

// ---------------------------------------------------------------------------
// Shared 3D noise helpers for the Sun shaders (no textures needed, no UV seams)
// ---------------------------------------------------------------------------

// Stable hash (Dave Hoskins) - returns 0..1
float3 SunHash33(float3 p)
{
    p = frac(p * float3(0.1031, 0.1030, 0.0973));
    p += dot(p, p.yxz + 33.33);
    return frac((p.xxy + p.yxx) * p.zyx);
}

// Quintic-interpolated gradient noise, roughly -0.8..0.8
float SunGradNoise(float3 p)
{
    float3 i = floor(p);
    float3 f = frac(p);
    float3 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);

    float n000 = dot(SunHash33(i + float3(0, 0, 0)) * 2.0 - 1.0, f - float3(0, 0, 0));
    float n100 = dot(SunHash33(i + float3(1, 0, 0)) * 2.0 - 1.0, f - float3(1, 0, 0));
    float n010 = dot(SunHash33(i + float3(0, 1, 0)) * 2.0 - 1.0, f - float3(0, 1, 0));
    float n110 = dot(SunHash33(i + float3(1, 1, 0)) * 2.0 - 1.0, f - float3(1, 1, 0));
    float n001 = dot(SunHash33(i + float3(0, 0, 1)) * 2.0 - 1.0, f - float3(0, 0, 1));
    float n101 = dot(SunHash33(i + float3(1, 0, 1)) * 2.0 - 1.0, f - float3(1, 0, 1));
    float n011 = dot(SunHash33(i + float3(0, 1, 1)) * 2.0 - 1.0, f - float3(0, 1, 1));
    float n111 = dot(SunHash33(i + float3(1, 1, 1)) * 2.0 - 1.0, f - float3(1, 1, 1));

    float x00 = lerp(n000, n100, u.x);
    float x10 = lerp(n010, n110, u.x);
    float x01 = lerp(n001, n101, u.x);
    float x11 = lerp(n011, n111, u.x);
    return lerp(lerp(x00, x10, u.y), lerp(x01, x11, u.y), u.z);
}

// Fractal Brownian motion, rotated octaves to avoid axis-aligned artifacts
float SunFbm(float3 p, int octaves)
{
    const float3x3 m = float3x3( 0.00,  0.80,  0.60,
                                -0.80,  0.36, -0.48,
                                -0.60, -0.48,  0.64);
    float sum = 0.0;
    float amp = 0.5;
    [loop]
    for (int i = 0; i < octaves; i++)
    {
        sum += amp * SunGradNoise(p);
        p = mul(m, p) * 2.02;
        amp *= 0.5;
    }
    return sum * 1.3; // approx -1..1
}

// Animated 3D Worley (cellular) noise. Returns (F1, F2) distances.
// F2 - F1 is ~0 along cell borders -> perfect for dark convection lanes.
float2 SunWorley(float3 p, float t)
{
    float3 ip = floor(p);
    float3 fp = frac(p);
    float f1 = 8.0;
    float f2 = 8.0;

    [unroll]
    for (int z = -1; z <= 1; z++)
    {
        [unroll]
        for (int y = -1; y <= 1; y++)
        {
            [unroll]
            for (int x = -1; x <= 1; x++)
            {
                float3 o = float3(x, y, z);
                float3 h = SunHash33(ip + o);
                // each feature point drifts around inside its cell -> boiling plasma
                float3 pt = o + 0.5 + 0.5 * sin(t + 6.2831853 * h) - fp;
                float d = dot(pt, pt);
                if (d < f1) { f2 = f1; f1 = d; }
                else if (d < f2) { f2 = d; }
            }
        }
    }
    return sqrt(float2(f1, f2));
}

#endif // SUN_NOISE_INCLUDED
