
#define PI 3.14159265358979323846



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

float2 EulerFormula(float x) {
    return float2(cos(x), sin(x));
}
// Helper function: complex multiplication of two complex numbers (a * b)
// where a and b are represented as float2 (x = real, y = imaginary)
float2 ComplexMultiplication(float2 a, float2 b)
{
    return float2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
}