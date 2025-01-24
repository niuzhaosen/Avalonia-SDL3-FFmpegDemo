// See https://aka.ms/new-console-template for more information
using SDL2;
if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
{
    Console.WriteLine($"There was an issue initializing SDL: {SDL.SDL_GetError()}");
    return;
}

IntPtr window = SDL.SDL_CreateWindow("SDL2-CS Example",
    SDL.SDL_WINDOWPOS_CENTERED,
    SDL.SDL_WINDOWPOS_CENTERED,
    800, 600,
    SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

if (window == IntPtr.Zero)
{
    Console.WriteLine($"There was an issue creating the window: {SDL.SDL_GetError()}");
    return;
}

bool running = true;
SDL.SDL_Event e;

while (running)
{
    while (SDL.SDL_PollEvent(out e) != 0)
    {
        if (e.type == SDL.SDL_EventType.SDL_QUIT)
        {
            running = false;
        }
    }
}

SDL.SDL_DestroyWindow(window);
SDL.SDL_Quit();