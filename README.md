# FFmpegTest
使用avalonia+ffmpeg.aotogen实现跨平台视频解码播放。
两种方式：
1.avalonia原生image控件刷新绘制。
    优点：可随意拖动窗体大小，绘制不受影响。
    缺点：对cpu内存占用较大。
2.内嵌sdl2进行渲染绘制。
    优点：对cpu内存占用较小。
    缺点：不可随意拖动窗体大小。若要拖动大小需要根据changesize获取大小重新创建window进行绘制。

只实现了软解方式，硬件加速解码后存在于显存中的数据想要直接绘制可以使用sdl方式，但需要对dx或vulkan库进行调用。（理论可行）