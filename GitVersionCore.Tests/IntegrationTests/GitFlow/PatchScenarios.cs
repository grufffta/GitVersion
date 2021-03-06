﻿using System.Linq;
using System.Threading;
using LibGit2Sharp;
using NUnit.Framework;

[TestFixture]
public class PatchScenarios
{
    [Test]
    public void PatchLatestReleaseExample()
    {
        using (var fixture = new BaseGitFlowRepositoryFixture("1.2.0"))
        {
            // create hotfix
            fixture.Repository.CreateBranch("hotfix-1.2.1").Checkout();

            fixture.AssertFullSemver("1.2.1-beta.1+0");
            fixture.Repository.MakeACommit();
            fixture.AssertFullSemver("1.2.1-beta.1+1");
            fixture.Repository.ApplyTag("1.2.1-beta.1");
            fixture.AssertFullSemver("1.2.1-beta.1+1");
            fixture.Repository.MakeACommit();
            fixture.AssertFullSemver("1.2.1-beta.2+2");

            // Merge hotfix branch to master
            fixture.Repository.Checkout("master");


            fixture.Repository.MergeNoFF("hotfix-1.2.1", Constants.SignatureNow());
            fixture.AssertFullSemver("1.2.1");

            fixture.Repository.ApplyTag("1.2.1");
            fixture.AssertFullSemver("1.2.1");

            // Verify develop version
            fixture.Repository.Checkout("develop");
            fixture.AssertFullSemver("1.3.0-unstable.0+0");

            // Warning: Hack-ish hack
            //
            // Ensure the merge commit is done at a different time than the previous one
            // Otherwise, as they would have the same content and signature, the same sha would be generated.
            // Thus 'develop' and 'master' would point at the same exact commit and the Assert below would fail.
            Thread.Sleep(1000);
            fixture.Repository.MergeNoFF("hotfix-1.2.1", Constants.SignatureNow());

            fixture.AssertFullSemver("1.3.0-unstable.1+1");
        }
    }

    [Test]
    public void PatchOlderReleaseExample()
    {
        using (var fixture = new BaseGitFlowRepositoryFixture(r =>
        {
            r.MakeATaggedCommit("1.0.0");
            r.MakeATaggedCommit("1.1.0");
            r.MakeATaggedCommit("1.2.0");
        }))
        {

            // create hotfix branch
            fixture.Repository.CreateBranch("hotfix-1.1.1", (Commit) fixture.Repository.Tags.Single(t => t.Name == "1.1.0").Target).Checkout();

            fixture.AssertFullSemver("1.1.1-beta.1+0");
            fixture.Repository.MakeACommit();
            fixture.AssertFullSemver("1.1.1-beta.1+1");

            // Merge hotfix branch to support
            fixture.Repository.CreateBranch("support-1.2", (Commit) fixture.Repository.Tags.Single(t => t.Name == "1.1.0").Target).Checkout();
            fixture.AssertFullSemver("1.1.0");

            fixture.Repository.MergeNoFF("hotfix-1.1.1", Constants.SignatureNow());
            fixture.AssertFullSemver("1.1.1");

            fixture.Repository.ApplyTag("1.1.1");
            fixture.AssertFullSemver("1.1.1");

            // Verify develop version
            fixture.Repository.Checkout("develop");
            fixture.AssertFullSemver("1.3.0-unstable.0+0");
        }
    }
}