using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Extensions.IO;
using AsmodatStandard.Extensions.Collections;
using System.IO;
using System.Threading;
using AsmodatStandard.Networking;
using AWSWrapper.SM;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CDHelper
{
    public partial class Program
    {
        private static async Task executeEmail(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "send":
                    {
                        var from = nArgs["from"];
                        var to = nArgs["to"];
                        var body = nArgs["body"];
                        var subject = nArgs["subject"];
                        var attachments = nArgs.GetValueOrDefault("attachments")?.Split(",");
                        var secret = nArgs.GetValueOrDefault("aws-secret");
                        var html = nArgs.GetValueOrDefault("html").ToBoolOrDefault(false);
                        var recursive = nArgs.GetValueOrDefault("recursive").ToBoolOrDefault(false);
                        var throwIfAttachementNotFound = nArgs.GetValueOrDefault("throwIfAttachementNotFound").ToBoolOrDefault(false);
                        SmtpMailSetup smtpSecret;
                        if (!secret.IsNullOrEmpty())
                        {
                            var sm = new SMHelper();
                            var sgs = await sm.GetSecret(name: secret);
                            smtpSecret = sgs.JsonDeserialize<SmtpMailSetup>();                           
                        }
                        else
                            throw new Exception("Credentials, e.g aws-secret was not specfied");
                         var smtpm = new SmtpMail(smtpSecret);

                        if (!body.IsNullOrEmpty())
                        {
                            var file = PathEx.ToRuntimePath(body);
                            if (File.Exists(file))
                            {
                                var fileBody = File.ReadAllText(file);
                                if (!fileBody.IsNullOrEmpty())
                                    body = fileBody;
                            }
                        }

                        await smtpm.Send(
                            from, 
                            to, 
                            body, 
                            subject, 
                            isBodyHTML: html, 
                            attachments: attachments, 
                            recursive: recursive,
                            throwIfAttachementNotFound: throwIfAttachementNotFound);

                        if(!_silent)
                            Console.WriteLine($"SUCCESS, Email was send to {to ?? "undefined"}. ");
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Emailing",
                    ("sent", "Accepts params: aws-secret, from, to, subject, body, html (default: false), recursive (default: false)"));
                    break;
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown Email command: '{args[0]} {args[1]}'");
                    }
            }
        }
    }
}
