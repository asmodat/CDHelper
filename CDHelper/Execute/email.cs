using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Extensions.IO;
using AsmodatStandard.Extensions.Collections;
using System.IO;
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
                        var to = nArgs["to"];
                        var body = nArgs.GetValueOrDefault("body", @default: "");
                        var subject = nArgs.GetValueOrDefault("subject", @default: "");
                        var attachments = nArgs.GetValueOrDefault("attachments")?.Split(",");
                        var secret = nArgs.GetValueOrDefault("aws-secret");
                        var envSecret = nArgs.GetValueOrDefault("env-secret", @default: "SMTP_SECRET");
                        var html = nArgs.GetValueOrDefault("html").ToBoolOrDefault(false);
                        var recursive = nArgs.GetValueOrDefault("recursive").ToBoolOrDefault(false);
                        var throwIfAttachementNotFound = nArgs.GetValueOrDefault("throwIfAttachementNotFound").ToBoolOrDefault(false);
                        var throwIfAttachementTooBig = nArgs.GetValueOrDefault("throwIfAttachementTooBig").ToBoolOrDefault(false);
                        var attachementMaxSize = nArgs.GetValueOrDefault("throwIfAttachementTooBig").ToIntOrDefault(25 * 1024 * 1024);
                        SmtpMailSetup smtpSecret = null;

                        if (secret.IsNullOrEmpty())
                        {
                            secret = envSecret.ContainsAll("{","}","host","port") ? envSecret : Environment.GetEnvironmentVariable(envSecret);
                            if (!secret.IsNullOrEmpty())
                                smtpSecret = secret.JsonDeserialize<SmtpMailSetup>();
                        }
                        else
                        {
                            var sm = new SMHelper();
                            var sgs = await sm.GetSecret(name: secret);
                            smtpSecret = sgs.JsonDeserialize<SmtpMailSetup>();
                        }

                        if (smtpSecret == null)
                            throw new Exception("SMTP secret (aws-secret or env-secret) was NOT specified");

                        var from = nArgs.GetValueOrDefault("from", @default: null) ?? smtpSecret.login;

                        if(!from.IsValidEmailAddress())
                            throw new Exception($"From property ({from ?? "null"}) is not a valid email addess");

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
                            throwIfAttachementNotFound: throwIfAttachementNotFound,
                            throwIfAttachementTooBig: throwIfAttachementTooBig,
                            attachementMaxSize: attachementMaxSize);

                        WriteLine($"SUCCESS, Email was send to {to ?? "undefined"}. ");
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Emailing",
                    ("sent", "Accepts params: aws-secret, from, to, subject, body, html (default: false), recursive (default: false), throwIfAttachementNotFound (default: false), throwIfAttachementTooBig (default: false), attachementMaxSize (default: 26214400)"));
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
