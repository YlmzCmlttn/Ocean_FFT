# Ocean_FFT

Ocean_FFT is an advanced ocean simulation engine that leverages Fast Fourier Transform (FFT) techniques and other state-of-the-art algorithms to create realistic water surfaces. The project implements a range of features to simulate dynamic wave behavior and surface details, and it is structured for further enhancements.

## Features & Implementations

### FFT Implementation
Utilizes the Fast Fourier Transform to convert spatial data into the frequency domain and back. This efficient method enables real-time simulation of complex wave patterns.

### High Frequency Damping
Implements a damping algorithm that reduces the amplitude of high-frequency components. This results in smoother waves and minimizes visual noise.

### Choppiness Fix with Horizontal Displacement
Addresses artifacts in wave simulations by applying horizontal displacement. This adjustment refines the appearance of wave choppiness for a more natural look.

### DFT Normal and Height Calculation
Uses the Discrete Fourier Transform to calculate accurate surface normals and heights, improving lighting, shading, and overall visual realism.

### Phillips Spectrum
Models the energy distribution of ocean waves based on the Phillips Spectrum, ensuring that the generated wave patterns are physically plausible.

### Reflection Transparency
Incorporates transparency in reflections to simulate realistic water surface effects, enhancing depth and visual complexity.

### Skybox Integration
Surrounds the scene with a skybox to provide an immersive background, reinforcing the environmental context of the simulation.

### Fog Effects
Adds atmospheric fog to the rendered scene, which helps convey depth and scale while adding to the visual ambiance.

### Fresnel Implementation
Applies Fresnel equations to simulate varying reflectivity at different viewing angles. This enhances the realism of the water surface by adjusting reflections based on the observer's perspective.

### Sum of Sines Implementation
Generates wave patterns by summing sine waves, offering an alternative approach for simulating diverse ocean conditions.

## References

- [Garrett Gunnell's Water Project](https://github.com/GarrettGunnell/Water)
- [FFT-Ocean by gasgiant](https://github.com/gasgiant/FFT-Ocean)

## Future Plans

- **Jonswap Implementation:** Integrate the Jonswap spectrum to further refine wave energy distribution.
- **Foam Simulation:** Develop dynamic foam effects for enhanced realism in breaking waves.
- **Tessellation Pass:** Implement a tessellation pass to increase detail on the ocean surface.
- **PBR Implementation:** Incorporate Physically Based Rendering techniques for more accurate material and lighting properties.