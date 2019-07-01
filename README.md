# VtNetCore.UWP - Virtual terminal for Universal Windows Platform

This is a "xterm compatible" embeddable control for Windows UWP apps. It is based on my other library VtNetCore.

The code here is mostly designed to be a reference for others to use when writing the view component of the VtNetCore control. I have been using this code more and more each day to work on Linux. There's always a little freedom when you write your own terminal application in the sense that when something doesn't work or look right, you an fix it.

The application of this code will be less interesting over time. This is because it appears that Microsoft is doing an amazing job at developing their own Terminal control as part of Microsoft Terminal which is expected to be a replacement for cmd.exe.

That said, the Microsoft terminal control, while amazing in most every way... at least as of today (1-July-2019), their code is barely able to be considered a VT100 library. They have almost no unit tests for escape codes and their interpretation of the escape codes is very weak.

They have done many things far better than I have, but I'm one guy and I wrote the code with very little time invested. Their code is definitely headed towards the commercial grade awesomeness that we would expect from what will be a core component of Windows.

From this point forward, most of my time invested in VtNetCore and VtNetCore.UWP will be to make an academic project as a platform for the development of compliance tests. Rather than being designed to be small and fast and just plain awesome, I will focus on developing a platform generic suite for a unit testing framework. 

This means I want to make a standardized interface that can be used to send data to a terminal and then query the terminal for status. This is comparable to what was done by the libvterm guys, but they made their own language which is difficult to parse and doesn't always account for scrolling buffers with history.

Now that I have a "tested and true" terminal parser, I intend to document (through code) the parser mechanism of a terminal. I was looking at Microsoft's Terminal code and loved their design, but I felt as though they were far too early in defining a state machine since it was clear from their code they didn't understand all the transitions that need to be considered. Also, by implementing a state machine as they have, it doesn't account for some of the more peculiar cases they will need to consider.

I think one of the biggest problems we have with modern terminal programs is that Thomas Dickey's control sequences and Ecma-48 are the only documents which describe the mechanisms and while they both do a great job of documenting what each control sequence does, there's no clean documentation that centralizes how to parse the incoming data. Instead, you are stuck with trial and error.

The state of this code will increase in stability as the focus is less oriented on features and is now focused on correctness. There are problems with how I handle XParseColor extensions (an XTerm extension) because it's a new feature and there are some ignored API hooks at this time (like cursor colors), but as I implement these, it will become more stable and more complete.

I'm torn at this time because .NET Core supports WPF which would make life REALLY easy for me to implement support for running local applications. UWP makes this very difficult. But instead, I might just bite the bullet and try to figure out how to run programs locally with proper permissions instead.

So, if you want to use this control in your application... please feel free and please file issues. It's pretty easy to use. There's an example which is pretty minimal and covers how to connect an SSH session to the terminal control.

## License

MIT
