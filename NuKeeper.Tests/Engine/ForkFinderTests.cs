﻿using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Engine;
using NuKeeper.Github;
using NUnit.Framework;
using Octokit;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class ForkFinderTests
    {
        [Test]
        public async Task FallbackForkIsUsedByDefault()
        {
            var fallbackFork = DefaultFork();

            var forkFinder = new ForkFinder(Substitute.For<IGithub>(), new NullNuKeeperLogger());

            var fork = await forkFinder.PushFork("testUser", "someRepo", fallbackFork);

            Assert.That(fork, Is.Not.Null);
            Assert.That(fork, Is.EqualTo(fallbackFork));
        }

        [Test]
        public async Task WhenUserForkIsFoundItIsUsed()
        {
            var fallbackFork = DefaultFork();

            var userRepo = MakeRepository();

            var github = Substitute.For<IGithub>();
            github.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github, new NullNuKeeperLogger());

            var fork = await forkFinder.PushFork("testUser", "someRepo", fallbackFork);

            Assert.That(fork, Is.Not.EqualTo(fallbackFork));
            AssertForkMatchesRepo(fork, userRepo);
        }

        [Test]
        public async Task WhenUserForkIsNotFoundItIsCreated()
        {
            var fallbackFork = DefaultFork();

            var userRepo = MakeRepository();

            var github = Substitute.For<IGithub>();
            github.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns((Repository)null);
            github.MakeUserFork(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github, new NullNuKeeperLogger());

            var actualFork = await forkFinder.PushFork("testUser", "someRepo", fallbackFork);

            await github.Received(1).MakeUserFork(Arg.Any<string>(), Arg.Any<string>());

            Assert.That(actualFork, Is.Not.Null);
            Assert.That(actualFork, Is.Not.EqualTo(fallbackFork));
        }

        private static Repository MakeRepository()
        {
            const string omniUrl = "http://somewhere.com";
            var owner = new User(omniUrl, "test user", null, 0, "test inc",
                DateTimeOffset.Now, 0, null, 0, 0, false, omniUrl, 1, 1,
                "testville", "testUser", "Testy",
                1, null, 0, 0,
                1, omniUrl, null, false, "test", null);


            var perms = new RepositoryPermissions(false, true, true);

            return new Repository(omniUrl, omniUrl, omniUrl, omniUrl, omniUrl, omniUrl, omniUrl,
                123, owner, "repoName", "repoName", "a test repo", omniUrl, "EN", false, false,
                1, 1, "master", 1, null, DateTimeOffset.Now, DateTimeOffset.Now, perms, null,
                null, false, false, false, false, 2, 122, true, true, true);
        }

        private ForkData DefaultFork()
        {
            return new ForkData(new Uri("http://someurl.com"), "testOrg", "someRepo");
        }

        private static void AssertForkMatchesRepo(ForkData fork, Repository repo)
        {
            Assert.That(fork, Is.Not.Null);
            Assert.That(fork.Name, Is.EqualTo(repo.Name));
            Assert.That(fork.Owner, Is.EqualTo(repo.Owner.Login));
            Assert.That(fork.Uri, Is.EqualTo(new Uri(repo.HtmlUrl)));
        }

    }
}
