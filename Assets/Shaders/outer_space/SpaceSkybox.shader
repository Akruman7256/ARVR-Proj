// Procedural deep-space skybox for URP (Unity 6). 100% math, no textures,
// so it is resolution independent (effectively "infinite K") and animated.
//
// Layers:
//   - Dark background + faint interstellar nebulosity
//   - Milky Way band: galactic bulge, structure, dark dust lanes, unresolved star grain
//   - Three star layers: power-law brightness, blackbody-style colours,
//     pixel-footprint anti-aliasing (no shimmering when the camera moves)
//   - Small emission / reflection / dark nebulae scattered over the sky
//   - Faint distant galaxies
//   - Comets with ion + dust tails that always point away from the Sun
//
// Assign in: Window > Rendering > Lighting > Environment > Skybox Material.
Shader "Custom/Space/Skybox"
{
    Properties
    {
        [Header(General)]
        _Exposure        ("Exposure", Range(0, 5)) = 1.0
        _RotationSpeed   ("Sky Rotation (deg/sec)", Range(-2, 2)) = 0.05
        [HDR] _BackgroundColor ("Background Colour", Color) = (0.0015, 0.002, 0.005, 1)

        [Header(Stars)]
        _StarIntensity       ("Star Intensity", Range(0, 10)) = 2.5
        _StarDensity         ("Star Density", Range(0, 4)) = 1.0
        _StarSize            ("Star Size", Range(0.02, 0.2)) = 0.07
        _StarBrightnessPower ("Faint Star Bias (higher = fewer bright)", Range(2, 20)) = 5
        _StarColorVariation  ("Star Colour Variation", Range(0, 1)) = 1.0

        [Header(Milky Way)]
        _GalaxyAxis       ("Galactic Plane Normal", Vector) = (0.25, 0.9, 0.35, 0)
        _GalaxyCenter     ("Galactic Centre Direction", Vector) = (0.85, -0.2, -0.5, 0)
        _MilkyWayIntensity("Milky Way Intensity", Range(0, 2)) = 0.45
        _BandWidth        ("Band Width", Range(0.03, 0.5)) = 0.14
        _BulgeSize        ("Bulge Size", Range(0.02, 0.6)) = 0.12
        _DustAmount       ("Dust Lanes", Range(0, 1.5)) = 0.8

        [Header(Nebulae)]
        _NebulaScale     ("Nebula Scale (higher = smaller, more)", Range(1, 12)) = 4
        _NebulaDensity   ("Nebula Density", Range(0, 1)) = 0.22
        _NebulaSize      ("Nebula Size", Range(0.3, 1.4)) = 1.0
        _NebulaIntensity ("Nebula Intensity", Range(0, 3)) = 0.7
        _NebulaFlow      ("Nebula Slow Boil", Range(0, 0.1)) = 0.01
        [HDR] _NebulaTint ("Nebula Tint", Color) = (1, 1, 1, 1)

        [Header(Distant Galaxies)]
        _GalaxyDensity   ("Distant Galaxy Density", Range(0, 0.1)) = 0.015
        _GalaxyBrightness("Distant Galaxy Brightness", Range(0, 3)) = 0.6

        [Header(Comets)]
        [IntRange] _CometCount ("Comet Count", Range(0, 8)) = 4
        _CometIntensity  ("Comet Intensity", Range(0, 5)) = 1.3
        _CometHeadSize   ("Comet Head Size (rad)", Range(0.001, 0.01)) = 0.0025
        _CometTailLength ("Tail Length", Range(0.3, 2)) = 1.0
        _CometPeriod     ("Seconds Per Comet Pass", Range(20, 600)) = 140
        _CometTravel     ("Distance Travelled (rad)", Range(0.1, 2.5)) = 0.7
        _FallbackSunDirection ("Fallback Sun Direction", Vector) = (0.5, 0.2, -0.8, 0)
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "SpaceSkybox"
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SpaceNoise.hlsl"

            // Set every frame by SpaceSkyDriver.cs (xyz = direction to the Sun, w = 1 when valid)
            float4 _SpaceSunDirection;

            CBUFFER_START(UnityPerMaterial)
                float  _Exposure;
                float  _RotationSpeed;
                float4 _BackgroundColor;

                float  _StarIntensity;
                float  _StarDensity;
                float  _StarSize;
                float  _StarBrightnessPower;
                float  _StarColorVariation;

                float4 _GalaxyAxis;
                float4 _GalaxyCenter;
                float  _MilkyWayIntensity;
                float  _BandWidth;
                float  _BulgeSize;
                float  _DustAmount;

                float  _NebulaScale;
                float  _NebulaDensity;
                float  _NebulaSize;
                float  _NebulaIntensity;
                float  _NebulaFlow;
                float4 _NebulaTint;

                float  _GalaxyDensity;
                float  _GalaxyBrightness;

                float  _CometCount;
                float  _CometIntensity;
                float  _CometHeadSize;
                float  _CometTailLength;
                float  _CometPeriod;
                float  _CometTravel;
                float4 _FallbackSunDirection;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 dirOS      : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.dirOS = v.positionOS.xyz;
                return o;
            }

            // ------------------------------------------------------------------
            // Stars
            // ------------------------------------------------------------------
            float3 SpaceStarColor(float t)
            {
                // 0 = hot blue-white, 1 = cool orange-red (skewed toward white/yellow by the hash)
                float3 blue   = float3(0.62, 0.74, 1.00);
                float3 white  = float3(1.00, 0.97, 0.92);
                float3 yellow = float3(1.00, 0.86, 0.62);
                float3 orange = float3(1.00, 0.62, 0.36);
                float3 c = lerp(blue, white, smoothstep(0.0, 0.35, t));
                c = lerp(c, yellow, smoothstep(0.30, 0.70, t));
                c = lerp(c, orange, smoothstep(0.65, 1.00, t));
                return c;
            }

            // Each cell of a 3D grid may hold one star. The sphere of view directions cuts
            // through the grid, so stars get naturally varied apparent brightness.
            float3 SpaceStarLayer(float3 d, float scale, float prob, float size, float power, float px, float seed)
            {
                float3 p  = d * scale;
                float3 id = floor(p);
                float3 h  = SpaceHash33(id + seed);
                if (h.x > prob) return 0;

                float3 h2 = SpaceHash33(id * 1.37 + seed + 19.19);
                float3 c  = id + 0.25 + 0.5 * h2;
                float3 v  = p - c;
                float  d2 = dot(v, v);

                // Anti-aliasing: a star can never be smaller than one pixel; when it is
                // widened we conserve its total energy so it does not flicker.
                float r0 = size;
                float rp = px * scale * 0.85;
                float r  = clamp(max(r0, rp), 0.001, 0.24);
                float energy = min(1.0, (r0 * r0) / (r * r));

                float b    = pow(h.y, power);
                float core = exp(-d2 / (r * r * 0.6));
                float dist = sqrt(d2);
                float halo = exp(-d2 / (r * r * 6.0)) * 0.07 * b;
                halo *= 1.0 - smoothstep(0.15, 0.25, dist); // avoid visible cell borders

                float3 col = SpaceStarColor(lerp(0.45, h2.z, _StarColorVariation));
                return col * (b * core * energy + halo);
            }

            // ------------------------------------------------------------------
            // Distant galaxies: faint tilted elliptical smudges
            // ------------------------------------------------------------------
            float3 SpaceDistantGalaxies(float3 d, float seed)
            {
                float  scale = 55.0;
                float3 p  = d * scale;
                float3 id = floor(p);
                float3 h  = SpaceHash33(id + seed);
                if (h.x > _GalaxyDensity) return 0;

                float3 h2 = SpaceHash33(id * 1.91 + seed + 3.3);
                float3 c  = id + 0.3 + 0.4 * h2;
                float3 v  = p - c;
                float3 a  = normalize(SpaceHash33(id + seed + 8.8) * 2.0 - 1.0 + 1e-4);

                float along = dot(v, a);
                float perp2 = dot(v, v) - along * along;
                float L  = 0.16 + 0.07 * h.y;
                float Wd = 0.07 + 0.05 * h.z;
                float e  = (along * along) / (L * L) + perp2 / (Wd * Wd);

                float core = exp(-e * 7.0);
                float disk = exp(-e * 1.6);
                float3 warm = float3(1.00, 0.88, 0.70);
                float3 cool = float3(0.65, 0.78, 1.00);
                return (warm * core * 1.2 + cool * disk * 0.45) * (0.5 + h.y) * _GalaxyBrightness * 0.5;
            }

            // ------------------------------------------------------------------
            // Nebulae: scattered 3D blobs, domain-warped fbm + ridged filaments
            // ------------------------------------------------------------------
            void SpaceNebulae(float3 d, float t, out float3 emission, out float absorb)
            {
                float3 np = d * _NebulaScale;
                float3 ic = floor(np);
                emission = 0;
                absorb   = 0;

                [loop]
                for (int z = -1; z <= 1; z++)
                {
                    [loop]
                    for (int y = -1; y <= 1; y++)
                    {
                        [loop]
                        for (int x = -1; x <= 1; x++)
                        {
                            float3 cid = ic + float3(x, y, z);
                            float3 h   = SpaceHash33(cid + 11.3);
                            if (h.x > _NebulaDensity) continue;

                            float3 h2  = SpaceHash33(cid + 5.7);
                            float3 c   = cid + 0.2 + 0.6 * h2;
                            float  rad = lerp(0.22, 0.6, h.y) * _NebulaSize;
                            float3 v   = np - c;
                            float  dd  = length(v) / rad;
                            if (dd >= 1.0) continue;

                            float3 q  = v / rad * 2.2 + h * 41.0 + float3(0.0, 0.0, t);
                            float  w1 = SpaceFbm(q * 0.8, 3);
                            float  n  = SpaceFbm(q * 1.5 + w1 * 1.4, 5) * 0.5 + 0.5;
                            float  rd = 1.0 - abs(SpaceFbm(q * 2.4 + w1 * 1.8, 4));

                            float fall = pow(saturate(1.0 - dd), 1.6);
                            float dens = smoothstep(0.30, 0.85, n) * fall;
                            float fil  = pow(saturate(rd), 3.0);
                            float glow = dens * (0.35 + 1.1 * fil);

                            float3 colA = 0, colB = 0;
                            bool darkNeb = false;
                            if (h.z < 0.40)      { colA = float3(1.00, 0.18, 0.30); colB = float3(0.10, 0.75, 0.85); } // H-alpha + OIII
                            else if (h.z < 0.70) { colA = float3(0.25, 0.45, 1.00); colB = float3(0.55, 0.85, 1.00); } // reflection
                            else if (h.z < 0.88) { colA = float3(1.00, 0.55, 0.18); colB = float3(0.90, 0.25, 0.55); } // warm
                            else                 { darkNeb = true; }                                                    // dark cloud

                            if (darkNeb)
                            {
                                absorb += dens * 1.4;
                            }
                            else
                            {
                                float3 colr = lerp(colA, colB, saturate(n * 1.3 - 0.3 + (1.0 - fall) * 0.3));
                                emission += colr * glow * (0.5 + h2.x);
                                absorb   += dens * 0.25;
                            }
                        }
                    }
                }
                emission *= _NebulaTint.rgb * _NebulaIntensity;
                absorb = saturate(absorb);
            }

            // ------------------------------------------------------------------
            // Comets: head + straight blue ion tail + curved yellow dust tail.
            // Tails point away from the Sun, like real comets.
            // ------------------------------------------------------------------
            float3 SpaceComets(float3 d, float time, float px, float3 sunDir)
            {
                float3 result = 0;
                int count = (int)(_CometCount + 0.5);

                [loop]
                for (int i = 0; i < 8; i++)
                {
                    if (i >= count) break;

                    float fi     = (float)i;
                    float period = _CometPeriod * (0.75 + 0.5 * frac(fi * 0.61803));
                    float phase  = time / period + frac(fi * 0.37 + 0.11);
                    float cycle  = floor(phase);
                    float t      = phase - cycle;

                    // New random comet each pass
                    float3 h  = SpaceHash33(float3(fi, cycle, 7.31));
                    float3 h2 = SpaceHash33(float3(cycle + 3.7, fi * 3.1, 19.7));

                    float3 a  = normalize(h * 2.0 - 1.0 + 1e-4);
                    float3 rv = h2 * 2.0 - 1.0;
                    float3 b  = normalize(rv - a * dot(rv, a) + 1e-4);

                    float travel = _CometTravel * (0.6 + 0.8 * h.x);
                    float ang    = (t - 0.5) * travel;
                    float3 headDir = a * cos(ang) + b * sin(ang);
                    float3 motion  = -a * sin(ang) + b * cos(ang);

                    float L = (0.10 + 0.14 * h.y) * _CometTailLength;
                    float rH = max(_CometHeadSize, px * 1.1);

                    float cosA = dot(d, headDir);
                    float maxAng = L * 1.6 + rH * 8.0;
                    if (cosA < cos(min(maxAng, 1.5))) continue;

                    // Tail direction: along the great circle, away from the Sun
                    float3 sPerp = sunDir - headDir * dot(sunDir, headDir);
                    float  sl    = length(sPerp);
                    float3 tailT = (sl > 1e-3) ? (-sPerp / sl) : b;
                    float3 side  = cross(headDir, tailT);

                    float3 vt = d - headDir * cosA;
                    float  u  = dot(vt, tailT);   // >0 along the tail
                    float  w  = dot(vt, side);    // across the tail

                    // Head (nucleus glare + coma)
                    float eH    = min(1.0, (_CometHeadSize * _CometHeadSize) / (rH * rH));
                    float dist2 = u * u + w * w;
                    float headCore = exp(-dist2 / (rH * rH * 0.4)) * eH * 5.0;
                    float coma     = exp(-sqrt(dist2) / (rH * 5.0)) * 0.3 * lerp(1.0, eH, 0.5);

                    float ion = 0.0;
                    float dust = 0.0;
                    if (u > 0.0)
                    {
                        // Ion tail: narrow, straight, bluish, streaky
                        float wi = max(rH * 0.5 + u * 0.010, px);
                        ion = exp(-u * 2.2 / L) * exp(-(w * w) / (2.0 * wi * wi)) * (rH * 0.5 / wi);
                        ion *= smoothstep(0.0, rH * 2.0, u);
                        float s = SpaceGradNoise(float3(u / L * 9.0 - time * 0.05, w / wi * 0.35, fi * 3.7)) * 0.5 + 0.5;
                        ion *= 0.55 + 0.9 * s;

                        // Dust tail: broader, yellowish, curves away from the direction of motion
                        float curveDir = (dot(motion, side) >= 0.0) ? -1.0 : 1.0;
                        float shift = curveDir * 0.8 * u * u / L;
                        float wd = max(rH * 0.8 + u * 0.06, px);
                        float ws = w - shift;
                        dust = exp(-u * 2.8 / L) * exp(-(ws * ws) / (2.0 * wd * wd)) * (rH * 0.8 / wd);
                        dust *= smoothstep(0.0, rH * 2.0, u);
                        float s2 = SpaceGradNoise(float3(u / L * 5.0, ws / wd * 0.4, fi * 5.1 + 9.0)) * 0.5 + 0.5;
                        dust *= 0.6 + 0.8 * s2;
                    }

                    float env    = smoothstep(0.0, 0.12, t) * (1.0 - smoothstep(0.82, 1.0, t));
                    float bright = _CometIntensity * (0.6 + 0.8 * h.z);

                    float3 headCol = float3(1.00, 0.97, 0.92);
                    float3 ionCol  = float3(0.40, 0.65, 1.00);
                    float3 dustCol = float3(1.00, 0.88, 0.68);
                    result += env * bright * (headCol * (headCore + coma) + ionCol * ion * 1.4 + dustCol * dust * 1.1);
                }
                return result;
            }

            // ------------------------------------------------------------------
            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float3 d  = normalize(IN.dirOS);
                float  px = max(length(fwidth(d)), 1e-5); // ~angular size of one pixel
                float  tm = _Time.y;
                float  nt = tm * _NebulaFlow;

                // Slow rotation of the deep sky (comets stay in world space)
                float rs, rc;
                sincos(radians(_RotationSpeed) * tm, rs, rc);
                float3 ds = float3(rc * d.x - rs * d.z, d.y, rs * d.x + rc * d.z);

                // ---- Background + faint interstellar nebulosity -----------------
                float3 col = _BackgroundColor.rgb;
                float3 wq  = ds * 1.4;
                float  bgn = SpaceFbm(wq + SpaceFbm(wq * 1.3 + 9.0, 3) * 0.8 + nt, 4) * 0.5 + 0.5;
                col += float3(0.10, 0.045, 0.16) * smoothstep(0.45, 0.95, bgn) * 0.10 * _NebulaIntensity;

                // ---- Milky Way -----------------------------------------------------
                float3 gAxis = normalize(_GalaxyAxis.xyz + 1e-5);
                float3 gCen  = _GalaxyCenter.xyz;
                gCen = normalize(gCen - gAxis * dot(gCen, gAxis) + 1e-5);

                float warp = SpaceFbm(ds * 3.0 + 5.0, 3) * 0.08;
                float lat  = dot(ds, gAxis) + warp;
                float bw2  = 2.0 * _BandWidth * _BandWidth;
                float band = exp(-(lat * lat) / bw2);
                float mid  = exp(-(lat * lat) / (bw2 * 0.12)); // thin mid-plane
                float cosC = dot(ds, gCen);
                float bulge = exp(-(1.0 - cosC) / max(_BulgeSize, 1e-3));

                float structure = SpaceFbm(ds * 4.0 + 17.0, 5) * 0.5 + 0.5;
                float fine      = SpaceFbm(ds * 11.0 + 3.0, 4) * 0.5 + 0.5;
                float ridged    = 1.0 - abs(SpaceFbm(ds * 6.5 + 41.0 + warp * 3.0, 5));
                float dust      = smoothstep(0.52, 0.92, ridged) * (mid * 0.9 + band * 0.35) * _DustAmount;
                float extinction = saturate(1.0 - dust * 0.85);

                float glow = band * (0.25 + 0.75 * saturate(structure * fine * 1.4)) + bulge * (0.9 + 0.6 * structure);
                float3 mwCol = lerp(float3(0.55, 0.68, 1.00), float3(1.00, 0.82, 0.58), saturate(bulge * 1.3 + 0.15));
                float3 milky = mwCol * glow * _MilkyWayIntensity * extinction;

                // Unresolved star grain inside the band (fades out when a pixel covers it)
                float grainFade = 1.0 - smoothstep(0.15, 0.5, px * 260.0);
                float grain = pow(saturate(SpaceGradNoise(ds * 260.0) * 0.7 + 0.5), 2.0);
                milky *= lerp(1.0, 0.6 + 0.8 * grain, grainFade);

                // ---- Nebulae -----------------------------------------------------------
                float3 nebEmit;
                float  nebAbs;
                SpaceNebulae(ds, nt, nebEmit, nebAbs);
                milky *= 1.0 - nebAbs * 0.8;

                // ---- Stars -------------------------------------------------------------
                float starBoost = 1.0 + 3.0 * band + 4.0 * bulge; // more stars in the galactic plane
                float pw = _StarBrightnessPower;
                float3 stars = 0;
                stars += SpaceStarLayer(ds,  28.0, 0.25 * _StarDensity * starBoost, _StarSize * 1.6, pw * 0.7, px, 0.0);
                stars += SpaceStarLayer(ds,  62.0, 0.12 * _StarDensity * starBoost, _StarSize * 1.2, pw,       px, 13.0);
                stars += SpaceStarLayer(ds, 140.0, 0.08 * _StarDensity * starBoost, _StarSize,       pw * 1.2, px, 29.0);
                stars *= _StarIntensity;
                float extAll = extinction * (1.0 - nebAbs * 0.9);
                stars *= lerp(1.0, extAll, 0.85);

                // ---- Distant galaxies ----------------------------------------------------
                float3 galaxies = SpaceDistantGalaxies(ds, 57.0) * extAll;

                col += milky + stars + galaxies + nebEmit;

                // ---- Comets (world space, tails point away from the Sun) -------------
                float3 sunDir = (_SpaceSunDirection.w > 0.5)
                              ? normalize(_SpaceSunDirection.xyz)
                              : normalize(_FallbackSunDirection.xyz + 1e-5);
                col += SpaceComets(d, tm, px, sunDir);

                col *= _Exposure;
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
