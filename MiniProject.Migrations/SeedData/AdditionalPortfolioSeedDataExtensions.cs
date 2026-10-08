using Microsoft.EntityFrameworkCore;
using MiniProject.Migrations.Entities;

namespace MiniProject.Migrations.SeedData;

internal static class AdditionalPortfolioSeedDataExtensions
{
    private sealed record ProfileSeed(
        string Name,
        string Headline,
        string Bio,
        string Location,
        string Handle,
        string CurrentCompany,
        string CurrentRole,
        string PreviousCompany,
        string PreviousRole);

    private static readonly ProfileSeed[] Profiles =
    [
        new("Jordan Brooks", "Product-minded software engineer", "I build calm, dependable tools for busy teams, with an eye for the small details that make software feel clear.", "Portland, OR", "jordan-brooks", "Cedarline", "Senior Software Engineer", "Maple Systems", "Software Engineer"),
        new("Priya Nair", "Full-stack developer | Digital health", "I turn complicated service journeys into approachable products and enjoy working closely with people who use what I build.", "Vancouver, BC", "priya-nair", "Wellnest", "Full-stack Developer", "CarePath", "Application Developer"),
        new("Mateo Alvarez", "Backend engineer | Distributed systems", "I like making reliable systems observable and understandable, especially when many services and people meet in one workflow.", "Austin, TX", "mateo-alvarez", "Orbit Works", "Backend Engineer", "Copperfield", "Software Developer"),
        new("Olivia Martin", "UX-focused frontend engineer", "I build accessible interfaces that help people move through complex information without losing the thread.", "Montreal, QC", "olivia-martin", "Northwind Studio", "Frontend Engineer", "Brightside Digital", "UI Developer"),
        new("Ethan Kim", "Software developer | Developer tools", "I enjoy improving the everyday experience of building, testing, and shipping software for small product teams.", "San Jose, CA", "ethan-kim", "Buildcraft", "Developer Experience Engineer", "Pixel Forge", "Software Engineer"),
        new("Sofia Rossi", "Data engineer | Climate technology", "I work on trustworthy data products that help researchers and communities understand a changing environment.", "Denver, CO", "sofia-rossi", "Terrain Labs", "Data Engineer", "OpenField", "Research Developer"),
        new("Noah Patel", "Cloud engineer | Reliable services", "I help teams ship safely by making infrastructure repeatable, observable, and easier to operate.", "Calgary, AB", "noah-patel", "Cloud Harbor", "Cloud Engineer", "Redwood Apps", "Platform Developer"),
        new("Amara Okafor", "Software engineer | Civic technology", "I care about useful public-interest software and build tools that make community information easier to reach.", "Chicago, IL", "amara-okafor", "Common Ground", "Software Engineer", "CityLab", "Web Developer"),
        new("Lucas Bennett", "Mobile and web developer", "I create practical mobile-first experiences, from the first sketch through launch and iteration.", "Boston, MA", "lucas-bennett", "Harborlight", "Mobile Engineer", "Tandem Apps", "Junior Developer"),
        new("Mei Tanaka", "Application developer | Creative tools", "I build thoughtful creative software and enjoy bridging design systems, product needs, and implementation details.", "Seattle, WA", "mei-tanaka", "Mosaic Studio", "Application Developer", "Paper Kite", "Frontend Developer"),
        new("Gabriel Santos", "Full-stack engineer | Collaboration products", "I make collaborative products feel simple, even when the underlying permissions, data, and workflows are not.", "Miami, FL", "gabriel-santos", "Relay Labs", "Full-stack Engineer", "Loopline", "Web Engineer"),
        new("Zara Ahmed", "Security-minded software engineer", "I help teams build secure-by-default products while keeping the user experience direct and approachable.", "Ottawa, ON", "zara-ahmed", "Lockstep", "Software Engineer", "Evergreen Tech", "Application Developer"),
        new("Caleb Foster", "Software engineer | Financial products", "I build clear financial tools that help people understand their options and make confident decisions.", "New York, NY", "caleb-foster", "Commonwealth", "Product Engineer", "Ledger House", "Software Developer"),
        new("Isabella Garcia", "Frontend engineer | Design systems", "I enjoy building reusable interface foundations that help product teams move faster without sacrificing accessibility.", "Phoenix, AZ", "isabella-garcia", "Kindred Design", "Frontend Engineer", "Canyon Software", "UI Engineer"),
        new("Aiden Wilson", "Data platform developer", "I make data pipelines and internal tools easier to trust, debug, and extend as a product grows.", "Edmonton, AB", "aiden-wilson", "Granite Data", "Data Platform Developer", "Signal Peak", "Data Developer"),
        new("Leila Haddad", "Software developer | Education", "I build inclusive learning experiences and care about giving educators useful feedback without adding busywork.", "Philadelphia, PA", "leila-haddad", "Learnwell", "Software Developer", "Open Classroom", "Web Developer"),
        new("Theo Nguyen", "Platform engineer | Open source", "I work on developer platforms and open-source projects that make the reliable path the easy path.", "San Diego, CA", "theo-nguyen", "Meridian Tools", "Platform Engineer", "Foss Harbor", "Software Engineer"),
        new("Nina Kowalski", "Full-stack developer | Local communities", "I create approachable digital services for local groups and the people who rely on them.", "Milwaukee, WI", "nina-kowalski", "Block & Bridge", "Full-stack Developer", "Civic Thread", "Application Developer"),
        new("Owen Clarke", "Software engineer | Mapping", "I build map-based products that make place, context, and useful local knowledge easier to explore.", "Halifax, NS", "owen-clarke", "Waypoint", "Software Engineer", "Atlas North", "GIS Developer"),
        new("Fatima Rahman", "Backend developer | Commerce", "I design resilient commerce services and enjoy smoothing out the small points of friction in a customer journey.", "Dallas, TX", "fatima-rahman", "Market Street", "Backend Developer", "Shopwell", "Software Engineer"),
        new("Henry Adams", "Product engineer | Remote collaboration", "I build tools for distributed teams and like turning scattered updates into shared understanding.", "Minneapolis, MN", "henry-adams", "Farview", "Product Engineer", "Workroom", "Full-stack Developer"),
        new("Camila Ferreira", "Software engineer | Arts and culture", "I help cultural organizations share their work online through accessible, expressive, and maintainable software.", "San Antonio, TX", "camila-ferreira", "Canvas House", "Software Engineer", "Public Gallery", "Web Developer"),
        new("Arjun Mehta", "Cloud developer | Observability", "I make production systems easier to understand with practical telemetry, useful dashboards, and careful automation.", "Waterloo, ON", "arjun-mehta", "Tracepoint", "Cloud Developer", "Blue Summit", "Software Engineer"),
        new("Grace Thompson", "Frontend developer | Public services", "I build accessible public-service interfaces that help people complete important tasks with confidence.", "Columbus, OH", "grace-thompson", "Civic Bridge", "Frontend Developer", "State Digital", "UI Developer"),
        new("Rafael Costa", "Software developer | Food systems", "I build tools that connect local food networks, independent producers, and the communities around them.", "Orlando, FL", "rafael-costa", "Harvest Table", "Software Developer", "Local Basket", "Full-stack Developer"),
        new("Chloe Dubois", "Full-stack engineer | Travel", "I create useful travel products that help people plan flexible trips and find memorable local experiences.", "Quebec City, QC", "chloe-dubois", "Roamstead", "Full-stack Engineer", "Trailmark", "Web Developer"),
        new("Isaac Morgan", "Software engineer | Health platforms", "I build dependable health software with careful attention to privacy, clarity, and the needs of care teams.", "Nashville, TN", "isaac-morgan", "Juniper Health", "Software Engineer", "Wellnest", "Backend Developer"),
        new("Elena Petrova", "Developer | Language learning", "I love building small, repeatable learning moments that help people practice a new language every day.", "Victoria, BC", "elena-petrova", "Lingua Loop", "Product Developer", "Wordgarden", "Frontend Developer"),
        new("David Okoye", "Backend engineer | Logistics", "I improve the software behind moving goods, coordinating people, and responding when plans change.", "Atlanta, GA", "david-okoye", "Routecraft", "Backend Engineer", "Freightline", "Software Developer"),
        new("Mina Park", "Software developer | Personal finance", "I build friendly financial tools that turn everyday decisions into manageable, understandable steps.", "Irvine, CA", "mina-park", "Pocketwise", "Software Developer", "Clearpath Finance", "Frontend Engineer"),
        new("Samuel Reed", "Mobile developer | Outdoor recreation", "I make outdoor planning tools that work well on the move and remain useful when connectivity is limited.", "Boise, ID", "samuel-reed", "Trailmark", "Mobile Developer", "Summit Apps", "Software Engineer"),
        new("Aisha Yusuf", "Data developer | Nonprofit technology", "I help mission-driven teams make better use of their data without losing sight of the people behind it.", "Detroit, MI", "aisha-yusuf", "Goodworks", "Data Developer", "Community Metrics", "Analyst Developer"),
        new("Julian Meyer", "Software engineer | Media platforms", "I build publishing and media tools that make it easier for creators to organize, distribute, and learn from their work.", "Los Angeles, CA", "julian-meyer", "Signal House", "Software Engineer", "Studio North", "Web Developer"),
        new("Hana Suzuki", "Frontend engineer | Accessibility", "I turn accessibility principles into practical interface patterns and help teams include more people by default.", "Boulder, CO", "hana-suzuki", "Open Door Labs", "Frontend Engineer", "Kindred Design", "UI Developer"),
        new("Benjamin Lee", "Full-stack developer | Small businesses", "I build pragmatic software for small businesses, focusing on useful workflows and low-maintenance operations.", "Richmond, BC", "benjamin-lee", "Main Street Software", "Full-stack Developer", "Cornerstone Apps", "Software Developer"),
        new("Layla Hassan", "Software engineer | Sustainability", "I work on digital products that help people and organizations make more sustainable everyday choices.", "Tucson, AZ", "layla-hassan", "Greenline", "Software Engineer", "Earthwise Digital", "Application Developer")
    ];

    private static readonly int[][] SkillSets =
    [
        [304, 305, 302, 303, 306],
        [311, 312, 307, 308, 309],
        [301, 314, 302, 313, 306],
        [315, 305, 316, 308, 317],
        [318, 319, 320, 321, 309],
        [322, 323, 307, 324, 325]
    ];

    private static readonly int[] ProjectIds = [101, 102, 103, 104, 105, 106, 107, 108];

    public static ModelBuilder HasAdditionalPortfolioSeedData(this ModelBuilder modelBuilder)
    {
        var profiles = new List<PortfolioProfile>();
        var projects = new List<ProfileProject>();
        var experiences = new List<PortfolioExperience>();
        var skills = new List<ProfileSkill>();
        var links = new List<PortfolioSocialLink>();

        for (var index = 0; index < Profiles.Length; index++)
        {
            var data = Profiles[index];
            var profileId = index + 3;

            profiles.Add(new PortfolioProfile
            {
                Id = profileId,
                DisplayName = data.Name,
                Headline = data.Headline,
                Bio = data.Bio,
                Location = data.Location,
                ContactEmail = $"{data.Handle}@example.com",
                ProfileImageUrl = $"https://images.example.com/profiles/{data.Handle}.jpg",
                ResumeUrl = $"https://example.com/resumes/{data.Handle}.pdf"
            });

            var sharedProjectIndexes = new[] { index % ProjectIds.Length, (index + 3) % ProjectIds.Length };
            for (var projectOrder = 0; projectOrder < sharedProjectIndexes.Length; projectOrder++)
            {
                var startDate = new DateOnly(2020 + index % 5, 1 + index % 12, 1);
                projects.Add(new ProfileProject
                {
                    ProfileId = profileId,
                    ProjectId = ProjectIds[sharedProjectIndexes[projectOrder]],
                    Role = projectOrder == 0 ? data.CurrentRole : data.PreviousRole,
                    Organization = projectOrder == 0 ? data.CurrentCompany : data.PreviousCompany,
                    StartDate = startDate,
                    EndDate = startDate.AddMonths(6),
                    IsFeatured = projectOrder == 0,
                    DisplayOrder = projectOrder
                });
            }

            var startYear = 2018 + index % 5;
            experiences.Add(new PortfolioExperience
            {
                Id = 2000 + index * 2,
                ProfileId = profileId,
                Company = data.CurrentCompany,
                JobTitle = data.CurrentRole,
                Description = $"Owned delivery of customer-facing features, partnered with design and product, " +
                              $"and improved the reliability of {data.CurrentCompany}'s core workflows.",
                StartDate = new DateOnly(startYear + 2, 1 + index % 12, 1),
                EndDate = null,
                IsCurrent = true,
                DisplayOrder = 0
            });
            experiences.Add(new PortfolioExperience
            {
                Id = 2001 + index * 2,
                ProfileId = profileId,
                Company = data.PreviousCompany,
                JobTitle = data.PreviousRole,
                Description = $"Built and maintained product features at {data.PreviousCompany}, learning to " +
                              "ship in small increments and support software after release.",
                StartDate = new DateOnly(startYear, 1 + (index + 4) % 12, 1),
                EndDate = new DateOnly(startYear + 1, 12, 1),
                IsCurrent = false,
                DisplayOrder = 1
            });

            var skillSet = SkillSets[index % SkillSets.Length];
            for (var skillIndex = 0; skillIndex < skillSet.Length; skillIndex++)
            {
                skills.Add(new ProfileSkill
                {
                    ProfileId = profileId,
                    SkillId = skillSet[skillIndex],
                    DisplayOrder = skillIndex
                });
            }

            links.Add(new PortfolioSocialLink
            {
                Id = 4000 + index * 2,
                ProfileId = profileId,
                Label = "GitHub",
                Url = $"https://github.com/{data.Handle}",
                DisplayOrder = 0
            });
            links.Add(new PortfolioSocialLink
            {
                Id = 4001 + index * 2,
                ProfileId = profileId,
                Label = "LinkedIn",
                Url = $"https://www.linkedin.com/in/{data.Handle}",
                DisplayOrder = 1
            });
        }

        modelBuilder.Entity<PortfolioProfile>().HasData(profiles);
        modelBuilder.Entity<ProfileProject>().HasData(projects);
        modelBuilder.Entity<PortfolioExperience>().HasData(experiences);
        modelBuilder.Entity<ProfileSkill>().HasData(skills);
        modelBuilder.Entity<PortfolioSocialLink>().HasData(links);

        return modelBuilder;
    }
}
