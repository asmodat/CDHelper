using System;
using AsmodatStandard.IO;
using AsmodatStandard.Extensions.Threading;
using AsmodatStandard.Extensions.Collections;
using AsmodatBitbucket;

namespace CDHelper
{
    public partial class Program
    {
        private static void executeBitbucket(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "pull-approve":
                    {
                        var key = nArgs["key"];
                        var secret = nArgs["secret"];
                        var username = nArgs["username"];
                        var slug = nArgs["slug"];
                        var source = nArgs.GetValueOrDefault("source", "develop");
                        var destination = nArgs.GetValueOrDefault("destination", "master");
                        var state = nArgs.GetValueOrDefault("state", "OPEN");

                        var bc = new BitbucketClient(key, secret);
                        var pullRequest = bc.GetPullRequest(username, slug, source, destination, state).Result;

                        if (pullRequest == null)
                            Console.WriteLine($"Warning, could not pull-approve because no pull request was found for source: {source}, destination: {destination}, state: {state} in {username}/{slug} repository.");

                        if (bc.PullRequestApprove(pullRequest).Result)
                            Console.WriteLine($"Success, Pull request {source} => {destination} in {username}/{slug} repository was approved.");

                        Console.WriteLine($"Failure, Pull request {source} => {destination} in {username}/{slug} repository was NOT approved.");
                    }
                    ;break;
                case "pull-unapprove":
                    {
                        var key = nArgs["key"];
                        var secret = nArgs["secret"];
                        var username = nArgs["username"];
                        var slug = nArgs["slug"];
                        var source = nArgs.GetValueOrDefault("source", "develop");
                        var destination = nArgs.GetValueOrDefault("destination", "master");
                        var state = nArgs.GetValueOrDefault("state", "OPEN");

                        var bc = new BitbucketClient(key, secret);
                        var pullRequest = bc.GetPullRequest(username, slug, source, destination, state).Result;

                        if (pullRequest == null)
                            Console.WriteLine($"Warning, could not pull-unapprove because no pull request was found for source: {source}, destination: {destination}, state: {state} in {username}/{slug} repository.");

                        if (bc.PullRequestUnApprove(pullRequest).Result)
                        {
                            Console.WriteLine($"Success, Pull request {source} => {destination} in {username}/{slug} repository was unpproved.");
                            return;
                        }

                        Console.WriteLine($"Failure, Pull request {source} => {destination} in {username}/{slug} repository was NOT unapproved.");
                    }
                    ; break;
                case "pull-comment":
                    {
                        var key = nArgs["key"];
                        var secret = nArgs["secret"];
                        var username = nArgs["username"];
                        var slug = nArgs["slug"];
                        var source = nArgs.GetValueOrDefault("source", "develop");
                        var destination = nArgs.GetValueOrDefault("destination", "master");
                        var state = nArgs.GetValueOrDefault("state", "OPEN");
                        var content = nArgs["content"];

                        var bc = new BitbucketClient(key, secret);
                        var pullRequest = bc.GetPullRequest(username, slug, source, destination, state).Result;

                        if (pullRequest == null)
                            Console.WriteLine($"Warning, could not comment because no pull request was found for source: {source}, destination: {destination}, state: {state} in {username}/{slug} repository.");

                        bc.PullRequestComment(pullRequest, content).Await();
                        Console.WriteLine($"Success, Commented Pull request {source} => {destination} in {username}/{slug} repository with text: '{content}'.");
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Curl Like Web Requests",
                    ("pull-approve", "Accepts params: key, secret, username, slug, source (default: develop), destination (default: master), state (default: OPEN)"),
                    ("pull-unapprove", "Accepts params: key, secret, username, slug, source (default: develop), destination (default: master), state (default: OPEN)"),
                    ("pull-comment", "Accepts params: key, secret, username, slug, source (default: develop), destination (default: master), state (default: OPEN), content"));
                    break;
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown AES command: '{args[0]} {args[1]}'");
                    }
            }
        }
    }
}
