---
name: Question and Feature request
about: Raise a question or suggest an idea for this project
title: ''
labels: question
assignees: ''

---

This template aims to help you writing high-quality questions. Raising a high-quality question gives you the best bang for your buck, because it typically takes far less time before you get a satisfactory answer.

You should start with writing a title that summarizes the specific problem, question, or feature request. Your title should be specific. If you have trouble coming up with a title, consider writing the title last. Here are some examples of good and bad titles:

Bad titles:
* Simple Injector Confusion
* How to get container instance
* sitecore upgrade issue with Simple Injector

Good titles:
* How to prevent Simple Injector from resolving generated ASP.NET Core TagHelper classes?
* Singletons are caching ThreadAbortExceptions, requiring the application to be restarted
* Can the ASP.NET Core integration cross-wiring feature work the other way around?

Describe your problem, question, or feature in a clear and concise way. Please make sure you:

* Do not post a duplicate question on StackOverflow. If you raised a question with the `simple-injector` tag on StackOverflow, rest assured that we already read your question. Posting a duplicate won't make you get an answer faster.
* Provide a Minimal, Complete, and Verifiable example that reproduces the problem, illustrates your question, or shows the feature you are missing. (see: https://stackoverflow.com/help/mcve)
* Show what you've tried.
* Show a simplified, but realistic, representation of your application. We are not interested to see a large amount of source code, but DI-related questions are typically design questions and it is impossible to feedback on your design when you reduce your interfaces and classes to e.g. `IFoo`, `Flux` and `Baz`.
* Include complete stack trace including all exception details of the exception and all inner exceptions in case an exception is thrown.
* Do not include screen shots of code, but provide actual code (see: https://idownvotedbecau.se/imageofcode)
* Do ensure that code listing don't have a line width that exceeds 100 characters, as that would cause horizontal scrollbars, which make it harder to view the code.
* Do not include screen shots of exceptions, but paste the actual exception details (see: https://idownvotedbecau.se/imageofanexception/)
* Make sure that code and exception details are formatted correctly and ideally include the programming language (typically `c#`). This way GitHub will highlight the code automatically. (see: https://help.github.com/en/articles/creating-and-highlighting-code-blocks)
* Follow the guidelines given by Stack Overflow and https://idownvotedbecau.se/ about asking good questions.
* If you think a feature is missing, consider providing an example of a possible API.

TIP: If your question is related to the registration of multiple related classes, consider showing a manually constructed object graph in plain C#. This not only is useful as a mental model for yourself, it is a useful way of communicating to others what it is you are trying to achieve. This is often much harder to comprehend when just showing DI registrations. Here's an example of a "manually constructed object graph in plain C#:"

    new HomeController(
        new MySpecialDecorator(
            new SomeService(),
        new SomeOtherDecorator(
            new OtherService())))

Thank you for your time in writing a high-quality question. This is much appreciated.