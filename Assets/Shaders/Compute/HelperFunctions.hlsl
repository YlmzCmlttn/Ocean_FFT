
#define PI 3.14159265358979323846

float PhillipsSpectrum(float A,float windSpeed,float2 windDirection, float2 waveVector)
{
    float magnitudeOfWaveVector = length(waveVector);
    if(magnitudeOfWaveVector < 0.0001f) return 0.0f;

    float L = windSpeed * windSpeed / 9.8f;
    float2 unitVectorOfWindDirection = normalize(windDirection);
    float DotProduct_Of_Normalize_Both_WaveVector_And_WindDirection = dot(normalize(waveVector),unitVectorOfWindDirection);
    float k = magnitudeOfWaveVector;
    float kw = DotProduct_Of_Normalize_Both_WaveVector_And_WindDirection;
    
    return A * exp(-1.0f/((k*L)*(k*L))) / (k*k*k*k) * (kw*kw);
}

// Helper: Hash function for generating pseudorandom numbers from an integer
uint hash(uint x)
{
    x = (x << 13U) ^ x;
    return x * (x * x * 15731U + 789221U) + 1376312589U;
}

float hashFloat(uint n)
{
    // Convert the hashed value to a float in [0,1]
    return (hash(n) & 0x7fffffffU) / float(0x7fffffffU);
}


// Helper: Convert two uniform random numbers to a pair of Gaussian (normal)
// random numbers using the Box-Muller transform.
float2 UniformToGaussian(float u1, float u2)
{
    float R = sqrt(-2.0 * log(u1));
    float theta = 2.0 * PI * u2;
    return float2(R * cos(theta), R * sin(theta));
}