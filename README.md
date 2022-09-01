## Attention!

We have been in state of war with Russia since February 24th.
To help Ukraine achieve victory as soon as possible, please, ignore all russian products, companies, projects... Everything.

Also you may help Armed Forces of Ukraine here: https://bank.gov.ua/en/news/all/natsionalniy-bank-vidkriv-spetsrahunok-dlya-zboru-koshtiv-na-potrebi-armiyi

We will release our Motherland from russian invaders and save Europe from aggressive inhuman russian regime. I promise.
#### Glory to Ukraine!

# FastSearchLibrary (.NET Core)
The multithreading library that provides opportunity both to fast find files or directories using different search criteria and search text files by its content.

.NET Framework version is available [here](https://github.com/VladPVS/FastSearchLibrary ".NET Framework version").

[The MIF](https://github.com/VladPVS/The-MIF "The MIF search tool") file search tool is based on this library. You can [try](https://github.com/VladPVS/The-MIF/releases "Download The MIF") it if you want to estimate speed of work right now.

![Downloads](https://img.shields.io/github/downloads/VladPVS/FastSearchLibraryNetCore/total.svg)

## ADVANTAGES
* Library uses recursive search algorithm that is splitted on subtasks executing in thread pool
* **UnauthorizedAccessException** is never thrown while search is executed
* It's possible to choose different search criteria
* It's possible to stop search process when it is necessary
* It's possible to set different search paths at the same time
* **It's possible to search text files by its content**

## INSTALLATION
1. Download archive with last [release](https://github.com/VladPVS/FastSearchLibraryNetCore/releases "Last release")
2. Extract content to some directory.
3. Copy .dll and .xml files in directory of your project.
4. Add library to your project: Solution Explorer -> Dependencies -> Add Project Reference in context menu -> Browse
5. Add appropriate namespace: `using FastSearchLibrary;`
6. Set target framework version at least .NET Core 3.1

## CONTENT

Next classes provide search functionality:
* FileSearcher
* DirectorySearcher
* FileSearcherMultiple
* DirectorySearcherMultiple
* FileTextSearcher

## USE PRINCIPLES
### Basic opportunities
  * Classes `FileSearcher` and `DirectorySearcher` contain static methods that allow to execute search by different criteria.
  These methods return result only when they fully complete execution.
  * Methods that have "Fast" ending divide task on several 
  subtasks that execute simultaneously in thread pool.
  * Methods that have "Async" ending return Task and don't block the called thread.
  * First group of methods accepts 2 parameters: 
    * `string folder` - start search directory
    * `string pattern` - the search string to match against the names of files in path.
    This parameter can contain a combination of valid literal path and wildcard (* and ?)
    characters, but doesn't support regular expressions.
    
  Examples:
````csharp 
List<FileInfo> files = FileSearcher.GetFiles(@"C:\Users", "*.txt");
````
   Finds all `*.txt` files in `C:\Users` using one thread method.

````csharp
List<FileInfo> files = FileSearcher.GetFilesFast(@"C:\Users", "*SomePattern*.txt");
````
   Finds all files that match appropriate pattern using several threads in thread pool.

````csharp
Task<List<FileInfo>> task = FileSearcher.GetFilesFastAsync(@"C:\", "a?.txt");
````
   Finds all files that match appropriate pattern using several threads in thread pool as
   an asynchronous operation.
   
   * Second group of methods accepts 2 parameters:
     * `string folder` - start search directory
     * `Func<FileInfo, bool> isValid` - delegate that determines algorithm of file selection.
     
   Examples:
````csharp
Task<List<FileInfo>> task = FileSearcher.GetFilesFastAsync(@"D:\", (f) =>
{
    return (f.Name.Contains("Pattern") || f.Name.Contains("Pattern2")) && f.LastAccessTime >= new DateTime(2018, 3, 1) && f.Length > 1073741824;
});
````
   Finds all files that match appropriate conditions using several threads in thread pool as
   an asynchronous operation.
   
   You also can use regular expressions:
````csharp    
Task<List<FileInfo>> task = FileSearcher.GetFilesFastAsync(@"D:\", (f) =>
{
     return Regex.IsMatch(f.Name, @".*Imagine[\s_-]Dragons.*.mp3$");
}); 
````
   Finds all files that match appropriate regular expression using several thread in thread pool as
   an asynchronous operation.
   
 ### Advanced opportunities
   If you want to execute some complicated search with realtime result getting you should use instance of `FileSearcher` class,
   that has various constructor overloads.
   `FileSearcher` class includes next events:
   * `event EventHandler<FileEventArgs> FilesFound` - fires when next portion of files is found.
     Event includes `List<FileInfo> Files { get; }` property that contains list of finding files.
   * `event EventHandler<SearchCompleted> SearchCompleted` - fires when search process is completed or stopped. 
     Event includes `bool IsCanceled { get; }` property that contains value that defines whether search process stopped by calling
     `StopSearch()` method. 
    To get stop search process possibility one has to use constructor that accepts CancellationTokenSource parameter.
    
   Example:
  ````csharp    
    class Searcher
    {
        private static object locker = new object(); // locker object

        private FileSearcher searcher;

        List<FileInfo> files;

        public Searcher()
        {
            files = new List<FileInfo>(); // create list that will contain search result
        }

        public void StartSearch()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            // create tokenSource to get stop search process possibility

            searcher = new FileSearcher(@"C:\", (f) =>
            {
               return Regex.IsMatch(f.Name, @".*[iI]magine[\s_-][dD]ragons.*.mp3$"); 
            }, tokenSource);  // give tokenSource in constructor
 

            searcher.FilesFound += (sender, arg) => // subscribe on FilesFound event
            {
                lock (locker) // using a lock is obligatorily
                {
                    arg.Files.ForEach((f) =>
                    {
                        files.Add(f); // add the next part of the received files to the results list
                        Console.WriteLine($"File location: {f.FullName}, \nCreation.Time: {f.CreationTime}");
                    });

                    if (files.Count >= 10) // one can choose any stopping condition
                       searcher.StopSearch();
                }
            };

            searcher.SearchCompleted += (sender, arg) => // subscribe on SearchCompleted event
            {
                if (arg.IsCanceled) // check whether StopSearch() called
                    Console.WriteLine("Search stopped.");
                else
                    Console.WriteLine("Search completed.");

                Console.WriteLine($"Quantity of files: {files.Count}"); // show amount of finding files
            };

            searcher.StartSearchAsync();
            // start search process as an asynchronous operation that doesn't block the called thread
        }
    }
 ````
 Note that all `FilesFound` event handlers are not thread safe so to prevent result loosing one should use
 `lock` keyword as you can see in example above or use thread safe collection from `System.Collections.Concurrent` namespace.
 
 ### Extended opportunities
   There are 2 additional parameters that one can set. These are `handlerOption` and `suppressOperationCanceledException`.
   `ExecuteHandlers handlerOption` parameter represents instance of `ExecuteHandlers` enumeration that specifies where
   FilesFound event handlers are executed:  
   * `InCurrentTask` value means that `FileFound` event handlers will be executed in that task where files were found. 
   * `InNewTask` value means that `FilesFound` event handlers will be executed in new task.
    Default value is `InCurrentTask`. It is more preferably in most cases. `InNewTask` value one should use only if handlers execute
    very sophisticated work that takes a lot of time, e.g. parsing of each found file.
    
   `bool suppressOperationCanceledException` parameter determines whether necessary to suppress 
   OperationCanceledException.
   If `suppressOperationCanceledException` parameter has value `false` and StopSearch() method is called the `OperationCanceledException` 
   will be thrown. In this case you have to process the exception manually.
   If `suppressOperationCanceledException` parameter has value `true` and StopSearch() method is called the `OperationCanceledException` 
   is processed automatically and you don't need to catch it. 
   Default value is `true`.
   
   Example:
  ````csharp           
    CancellationTokenSource tokenSource = new CancellationTokenSource();

    FileSearcher searcher = new FileSearcher(@"D:\Program Files", (f) =>
    {
       return Regex.IsMatch(f.Name, @".{1,5}[Ss]ome[Pp]attern.txt$") && (f.Length >= 8192); // 8192b == 8Kb 
    }, tokenSource, ExecuteHandlers.InNewTask, true); // suppressOperationCanceledException == true
 ````
   ### Multiple search
   `FileSearcher` and `DirectorySearcher` classes can search only in one directory (and in all subdirectories surely) 
   but what if you want to perform search in several directories at the same time?     
   Of course, you can create some instances of `FileSearcher` (or `DirectorySearcher`) class and launch them simultaneously, 
   but `FilesFound` (or `DirectoriesFound`) events will occur for each instance you create. As a rule, it's inconveniently.
   Classes `FileSearcherMultiple` and `DirectorySearcherMultiple` are intended to solve this problem. 
   They are similar to `FileSearcher` and `DirectorySearcher` but can execute search in several directories.
   The difference between `FileSearcher` and `FileSearcherMultiple` is that constructor of `Multiple` class accepts list of 
   directories instead one directory.
   
   Example:
 ````csharp
    List<string> folders = new List<string>
    {
      @"C:\Users\Public",
      @"C:\Windows\System32",
      @"D:\Program Files",
      @"D:\Program Files (x86)"
    }; // list of search directories

    List<string> keywords = new List<string> { "word1", "word2", "word3" }; // list of search keywords

    FileSearcherMultiple multipleSearcher = new FileSearcherMultiple(folders, (f) =>
    {
       if (f.CreationTime >= new DateTime(2015, 3, 15) &&
          (f.Extension == ".cs" || f.Extension == ".sln"))
          {
             foreach (var keyword in keywords)
               if (f.Name.Contains(keyword))
                 return true;
          }
          
       return false;
    }, tokenSource, ExecuteHandlers.InCurrentTask, true);       
 ````
   ### Text search
   
  Class `FileTextSearcher` contain static methods that allow to execute search
  of text files by its content using the same multithreading algorithm.
  
  4 overloads of FileTextSearcher.SearchFilesByTextAsync() methods that accept:
  * Enumeration of start searching directories
  * Either delegate that filters files where is necessary to perform search by content
  or string pattern that can contain a combination of valid literal path 
  and wildcard (* and ?) characters, but doesn't support regular expressions.
  * Either delegate that determines whether file content corresponds the searching requirements
  or enumeration of searhing expressions
  * Encoding of text files
  * Value of StringComparison enum that determines how to compare searching expressions.
  Applyiable only to those method overloads that accept enumeration of searching expressions
  instead of delegate.   

  The difference between these method is represented by next example.
  They all search on C:\ and D:\ drives .txt files that contain at least one of
  the next expression: "first search expression", "second search expression" ignoring text case.  

  Example:
 ````csharp
    var foundFiles = await FileTextSearcher.SearchFilesByTextAsync(new[] { @"C:\", @"D:\" }, f => f.Extension == ".txt",
        t => new[] { "first search expression", "second search expression" }
            .Any(exp => t.Contains(exp, StringComparison.OrdinalIgnoreCase)),
        Encoding.Default);

    var foundFiles2 = await FileTextSearcher.SearchFilesByTextAsync(new[] { @"C:\", @"D:\" }, f => f.Extension == ".txt",
        new[] { "first search expression", "second search expression" },
        Encoding.Default, StringComparison.OrdinalIgnoreCase);

    var foundFiles3 = await FileTextSearcher.SearchFilesByTextAsync(new[] { @"C:\", @"D:\" }, "*.txt",
        t => new[] { "first search expression", "second search expression" }
            .Any(exp => t.Contains(exp, StringComparison.OrdinalIgnoreCase)),
        Encoding.Default);

    var foundFiles4 = await FileTextSearcher.SearchFilesByTextAsync(new[] { @"C:\", @"D:\" }, "*.txt",
        new[] { "first search expression", "second search expression" },
        Encoding.Default, StringComparison.OrdinalIgnoreCase);
````

There is a `FileTextSearcher` class if it's necessary to get real time result getting.

This is example for experiments of `FileTextSearcher` class using:

````csharp
    Console.WriteLine("Searching...");

    object locker = new object();

    Stopwatch stopwatch = Stopwatch.StartNew();
    stopwatch.Start();

    var files = new List<FileInfo>();

    var fileTextSearcher = new FileTextSearcher(
        new [] { @"C:\" },
        f => f.CreationTime > new DateTime(2022, 3, 1)
            && f.Extension.Equals(".txt", StringComparison.OrdinalIgnoreCase),
        t => new[] { "hello", "test" }.Any(exp => t.Contains(exp, StringComparison.OrdinalIgnoreCase))); 

    fileTextSearcher.FilesFound += (sender, e) =>
    {
        e.Files.ForEach(f => Console.WriteLine(f.FullName));

        lock (locker) // it's necessary to use "lock" or apply a concurrent collection
        {
            files.AddRange(e.Files);
        }
    };

    fileTextSearcher.SearchCompleted += (sender, e) =>
    {
        bool isCompleted = !e.IsCanceled;

        Console.WriteLine(isCompleted ? "Completed" : "Canceled");
    };

    var searchTask = fileTextSearcher.SearchAsync();

    Task task = Task.Run(async () =>
    {
        Console.WriteLine("Press F1 to cancel");

        while (!searchTask.IsCompleted)
        {
            if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.F1)
            {
                fileTextSearcher.StopSearch();
                return;
            }

            await Task.Delay(10);
        }
    });

    await searchTask;

    Console.WriteLine($"Spent time: {stopwatch.Elapsed.Minutes} min {stopwatch.Elapsed.Seconds} s {stopwatch.Elapsed.Milliseconds} ms");
    Console.WriteLine($"Files: {files.Count}");
````

It's possible to use regular expressions for search files by content,
but in practice it will take a lot of time. So if you have to use them, try to minimize quantity of files that need to be checked by
regular expression. Also this case one cannot expect using of `StopSearch()` method for cancelling operation as it can be performed unpredictable time.
Therefore, it's better to keep all your lambdas and event handlers as simple as possible.

Example:

````csharp
    var fileTextSearcher = new FileTextSearcher(
        new [] { @"C:\" },
        f => f.CreationTime > new DateTime(2022, 5, 1)
          && f.Extension.Equals(".txt", StringComparison.OrdinalIgnoreCase),
        t => Regex.IsMatch(t, $".*(first|second|third) search expression.*", RegexOptions.Compiled));  
````
