using Microsoft.EntityFrameworkCore;
using SkillifyAPI.Data;
using SkillifyAPI.Models;

public static class DataSeeder
{
    public static async Task Seed(AppDbContext context)
    {
        context.Database.Migrate();
        await SeedMainSkillsAsync(context);
        await SeedLanguagesAsync(context);
        await SeedBadgesAsync(context);
    }

    // ─────────────────────────────────────────────────────────
    //  MAIN SKILLS + SUB SKILLS SEEDER
    // ─────────────────────────────────────────────────────────
    private static async Task SeedMainSkillsAsync(AppDbContext context)
    {
        //if (context.MainSkills.Any()) return;   // guard — run once
        if (await context.MainSkills.AsNoTracking().AnyAsync())
            return;

        var slugHelper = new Slugify.SlugHelper();

        var data = new Dictionary<string, List<string>>
        {
            // 1
            ["Programming"] = new()
            {
                "Backend Development", "Frontend Development", "Mobile Development",
                "Game Development", "Embedded Systems", "DevOps & CI/CD",
                "API Design", "Compiler Design", "Code Review", "Open Source Contribution"
            },
            // 2
            ["Engineering"] = new()
            {
                "Civil Engineering", "Mechanical Engineering", "Electrical Engineering",
                "Software Engineering", "Chemical Engineering", "Aerospace Engineering",
                "Biomedical Engineering", "Environmental Engineering",
                "Industrial Engineering", "Structural Engineering"
            },
            // 3
            ["Teaching"] = new()
            {
                "Primary Education", "Secondary Education", "University Lecturing",
                "Online Course Creation", "Corporate Training", "Special Needs Education",
                "Language Teaching", "STEM Teaching", "Curriculum Design", "Educational Coaching"
            },
            // 4
            ["Design"] = new()
            {
                "UI Design", "UX Design", "Graphic Design", "Motion Design",
                "Product Design", "Brand Identity", "Illustration", "3D Modeling",
                "Typography", "Design Systems"
            },
            // 5
            ["Data Science"] = new()
            {
                "Data Analysis", "Machine Learning", "Deep Learning", "NLP",
                "Computer Vision", "Data Visualization", "Statistical Modeling",
                "Big Data Engineering", "Feature Engineering", "MLOps"
            },
            // 6
            ["Marketing"] = new()
            {
                "Digital Marketing", "SEO", "Content Marketing", "Social Media Marketing",
                "Email Marketing", "Influencer Marketing", "Performance Marketing",
                "Brand Management", "Market Research", "Growth Hacking"
            },
            // 7
            ["Finance"] = new()
            {
                "Accounting", "Investment Analysis", "Financial Planning",
                "Risk Management", "Auditing", "Tax Planning",
                "Corporate Finance", "Cryptocurrency", "Stock Trading", "Insurance"
            },
            // 8
            ["Healthcare"] = new()
            {
                "General Medicine", "Surgery", "Nursing", "Pharmacy",
                "Physical Therapy", "Mental Health Counseling", "Radiology",
                "Dentistry", "Nutrition & Dietetics", "Emergency Medicine"
            },
            // 9
            ["Law"] = new()
            {
                "Criminal Law", "Civil Law", "Corporate Law", "Intellectual Property",
                "Family Law", "Labor Law", "Tax Law", "International Law",
                "Environmental Law", "Human Rights Law"
            },
            // 10
            ["Architecture"] = new()
            {
                "Residential Design", "Commercial Design", "Urban Planning",
                "Interior Architecture", "Landscape Architecture", "Sustainable Design",
                "Restoration & Renovation", "BIM Modeling", "Construction Management", "Parametric Design"
            },
            // 11
            ["Photography"] = new()
            {
                "Portrait Photography", "Landscape Photography", "Wildlife Photography",
                "Event Photography", "Product Photography", "Street Photography",
                "Aerial Photography", "Photo Editing", "Videography", "Cinematography"
            },
            // 13
            ["Writing"] = new()
            {
                "Creative Writing", "Technical Writing", "Copywriting", "Blogging",
                "Journalism", "Screenwriting", "Academic Writing", "Grant Writing",
                "Ghostwriting", "Editing & Proofreading"
            },
            // 14
            ["Cybersecurity"] = new()
            {
                "Penetration Testing", "Network Security", "Application Security",
                "Cloud Security", "Incident Response", "Malware Analysis",
                "Cryptography", "Identity & Access Management", "Security Auditing", "Threat Intelligence"
            },
            // 15
            ["Cloud Computing"] = new()
            {
                "AWS", "Azure", "Google Cloud", "Cloud Architecture",
                "Serverless Computing", "Kubernetes", "Docker", "Cloud Networking",
                "Cloud Storage", "Cost Optimization"
            },
            // 16
            ["Project Management"] = new()
            {
                "Agile", "Scrum", "Kanban", "PMP",
                "Risk Management", "Stakeholder Management", "Resource Planning",
                "Budget Management", "Change Management", "Program Management"
            },
            // 17
            ["Human Resources"] = new()
            {
                "Talent Acquisition", "Onboarding", "Performance Management",
                "Payroll Management", "Employee Relations", "HR Analytics",
                "Learning & Development", "Compensation & Benefits", "Diversity & Inclusion", "Organizational Design"
            },
            // 18
            ["Sales"] = new()
            {
                "B2B Sales", "B2C Sales", "Inside Sales", "Field Sales",
                "Sales Strategy", "CRM Management", "Lead Generation",
                "Account Management", "Negotiation", "Sales Analytics"
            },
            // 19
            ["Customer Support"] = new()
            {
                "Technical Support", "Customer Success", "Live Chat Support",
                "Call Center Operations", "Complaint Resolution", "CRM Tools",
                "Ticket Management", "SLA Management", "Customer Feedback", "Help Desk"
            },
            // 20
            ["Artificial Intelligence"] = new()
            {
                "Prompt Engineering", "LLM Fine-Tuning", "AI Ethics",
                "Reinforcement Learning", "Generative AI", "AI Research",
                "Robotics", "Autonomous Systems", "Edge AI", "AI Product Management"
            },
            // 21
            ["Blockchain"] = new()
            {
                "Smart Contracts", "DeFi", "NFT Development", "Web3",
                "Solidity", "Consensus Mechanisms", "Tokenomics",
                "Crypto Wallet Development", "DAO Governance", "Layer 2 Solutions"
            },
            // 22
            ["Networking"] = new()
            {
                "Network Administration", "LAN/WAN", "VPN Configuration",
                "Firewall Management", "DNS & DHCP", "Network Monitoring",
                "SD-WAN", "Wireless Networking", "Network Troubleshooting", "Routing & Switching"
            },
            // 23
            ["Database Administration"] = new()
            {
                "SQL Server", "PostgreSQL", "MySQL", "Oracle DB",
                "MongoDB", "Redis", "Database Design", "Performance Tuning",
                "Backup & Recovery", "Data Migration"
            },
            // 24
            ["Business Analysis"] = new()
            {
                "Requirements Gathering", "Process Modeling", "Gap Analysis",
                "Use Case Writing", "Stakeholder Interviews", "SWOT Analysis",
                "Data Flow Diagrams", "Feasibility Studies", "Business Process Improvement", "Wireframing"
            },
            // 25
            ["Entrepreneurship"] = new()
            {
                "Startup Strategy", "Business Plan Writing", "Fundraising",
                "Pitching", "Product-Market Fit", "Lean Startup",
                "MVP Development", "Scaling", "Exit Strategy", "Franchise Management"
            },
            // 26
            ["Translation"] = new()
            {
                "Arabic to English", "French to English", "Spanish to English",
                "German to English", "Chinese to English", "Legal Translation",
                "Medical Translation", "Technical Translation", "Subtitling", "Localization"
            },
            // 27
            ["Public Speaking"] = new()
            {
                "Presentation Skills", "Storytelling", "Debate",
                "TED-Style Talks", "Conference Speaking", "MC & Hosting",
                "Motivational Speaking", "Pitching to Investors", "Toastmasters", "Voice Coaching"
            },
            // 28
            ["Research"] = new()
            {
                "Academic Research", "Market Research", "Scientific Research",
                "UX Research", "Policy Research", "Literature Review",
                "Quantitative Methods", "Qualitative Methods", "Survey Design", "Data Collection"
            },
            // 29
            ["Social Work"] = new()
            {
                "Child Welfare", "Community Outreach", "Mental Health Support",
                "Elderly Care", "Crisis Intervention", "Rehabilitation",
                "Case Management", "Addiction Counseling", "Domestic Violence Support", "NGO Management"
            },
            // 30
            ["Psychology"] = new()
            {
                "Clinical Psychology", "Cognitive Behavioral Therapy", "Child Psychology",
                "Organizational Psychology", "Neuropsychology", "Counseling",
                "Positive Psychology", "Behavioral Analysis", "Psychotherapy", "Forensic Psychology"
            },
            // 31
            ["Accounting"] = new()
            {
                "Bookkeeping", "Financial Reporting", "Auditing",
                "Tax Preparation", "Payroll", "Cost Accounting",
                "Forensic Accounting", "IFRS", "GAAP", "Management Accounting"
            },
            // 32
            ["Supply Chain"] = new()
            {
                "Logistics", "Procurement", "Inventory Management",
                "Warehouse Operations", "Demand Forecasting", "Supplier Relations",
                "Import & Export", "Last-Mile Delivery", "ERP Systems", "Green Supply Chain"
            },
            // 33
            ["Quality Assurance"] = new()
            {
                "Manual Testing", "Automated Testing", "Performance Testing",
                "Security Testing", "Test Planning", "Bug Tracking",
                "Selenium", "Cypress", "Load Testing", "API Testing"
            },
            // 34
            ["Embedded & IoT"] = new()
            {
                "Arduino", "Raspberry Pi", "RTOS",
                "Firmware Development", "Sensor Integration", "Industrial IoT",
                "Edge Computing", "Microcontroller Programming", "PCB Design", "MQTT Protocol"
            },
            // 35
            ["Agriculture"] = new()
            {
                "Crop Management", "Soil Science", "Irrigation Systems",
                "Livestock Management", "Organic Farming", "Precision Agriculture",
                "Agri-Tech", "Pest Control", "Greenhouse Management", "Agricultural Economics"
            },
            // 36
            ["Environmental Science"] = new()
            {
                "Climate Change", "Waste Management", "Water Treatment",
                "Air Quality Monitoring", "Environmental Impact Assessment",
                "Biodiversity Conservation", "Renewable Energy", "Carbon Footprint Analysis",
                "Environmental Policy", "GIS Mapping"
            },
            // 37
            ["Real Estate"] = new()
            {
                "Property Sales", "Property Valuation", "Real Estate Investment",
                "Property Management", "Leasing", "Commercial Real Estate",
                "Real Estate Law", "Mortgage Advisory", "Construction Project Management", "Urban Development"
            },
            // 38
            ["Culinary Arts"] = new()
            {
                "Pastry & Baking", "Fine Dining", "Street Food",
                "Nutrition Planning", "Food Safety", "Restaurant Management",
                "Catering", "Food Photography", "Recipe Development", "Barista Skills"
            },
            // 39
            ["Fashion"] = new()
            {
                "Fashion Design", "Textile Design", "Styling",
                "Fashion Marketing", "Tailoring", "Pattern Making",
                "Sustainable Fashion", "Fashion Photography", "Trend Forecasting", "Retail Buying"
            },
            // 40
            ["Sports & Fitness"] = new()
            {
                "Personal Training", "Strength & Conditioning", "Sports Coaching",
                "Yoga Instruction", "Pilates", "Nutrition for Athletes",
                "Sports Psychology", "Physical Rehabilitation", "CrossFit", "Martial Arts Instruction"
            },
            // 41
            ["Event Management"] = new()
            {
                "Corporate Events", "Weddings", "Conferences",
                "Concert Production", "Exhibition Management", "Virtual Events",
                "Event Marketing", "Venue Management", "Sponsorship Management", "On-Site Coordination"
            },
            // 42
            ["Animation"] = new()
            {
                "2D Animation", "3D Animation", "Motion Graphics",
                "Character Design", "Storyboarding", "Visual Effects",
                "Stop Motion", "CGI", "Rigging", "Rendering"
            },
            // 43
            ["Game Development"] = new()
            {
                "Unity", "Unreal Engine", "Game Design",
                "Level Design", "Game Programming", "Game Art",
                "Game Audio", "Multiplayer Networking", "AR & VR Games", "Mobile Game Development"
            },
            // 44
            ["Content Creation"] = new()
            {
                "YouTube", "TikTok", "Podcast Production",
                "Instagram Reels", "Blog Writing", "Newsletter Writing",
                "Twitch Streaming", "Video Editing", "Scripting", "Community Building"
            },
            // 45
            ["Consulting"] = new()
            {
                "Strategy Consulting", "IT Consulting", "Management Consulting",
                "Financial Consulting", "HR Consulting", "Legal Consulting",
                "Marketing Consulting", "Operations Consulting", "Change Management Consulting", "Risk Consulting"
            },
            // 46
            ["Biotechnology"] = new()
            {
                "Genetic Engineering", "Bioinformatics", "Drug Discovery",
                "Clinical Trials", "Bioprocessing", "CRISPR",
                "Molecular Biology", "Cell Culture", "Proteomics", "Genomics"
            },
            // 47
            ["DevOps"] = new()
            {
                "CI/CD Pipelines", "Infrastructure as Code", "Monitoring & Alerting",
                "Container Orchestration", "GitOps", "Site Reliability Engineering",
                "Configuration Management", "Secrets Management", "Platform Engineering", "Chaos Engineering"
            },
            // 48
            ["Product Management"] = new()
            {
                "Product Strategy", "Roadmap Planning", "User Story Writing",
                "Backlog Grooming", "A/B Testing", "OKRs & KPIs",
                "Competitive Analysis", "Go-to-Market Strategy", "Product Analytics", "Stakeholder Communication"
            },
            // 49
            ["Astronomy"] = new()
            {
                "Astrophysics", "Observational Astronomy", "Cosmology",
                "Planetary Science", "Space Exploration", "Telescope Operation",
                "Astrochemistry", "Exoplanet Research", "Space Mission Planning", "Radio Astronomy"
            },
            // 50
            ["Physics"] = new()
            {
                "Quantum Mechanics", "Thermodynamics", "Electromagnetism",
                "Optics", "Nuclear Physics", "Particle Physics",
                "Fluid Dynamics", "Solid State Physics", "Acoustics", "Theoretical Physics"
            },
            // 51
            ["Chemistry"] = new()
            {
                "Organic Chemistry", "Inorganic Chemistry", "Physical Chemistry",
                "Analytical Chemistry", "Biochemistry", "Polymer Science",
                "Electrochemistry", "Computational Chemistry", "Green Chemistry", "Materials Science"
            },
            // 52
            ["Mathematics"] = new()
            {
                "Calculus", "Linear Algebra", "Statistics",
                "Probability", "Discrete Mathematics", "Number Theory",
                "Topology", "Differential Equations", "Abstract Algebra", "Numerical Analysis"
            },
            // 53
            ["Languages"] = new()
            {
                "Arabic", "English", "French",
                "Spanish", "German", "Mandarin Chinese",
                "Japanese", "Portuguese", "Russian", "Italian"
            },
            // 54
            ["Philosophy"] = new()
            {
                "Ethics", "Logic", "Epistemology",
                "Metaphysics", "Political Philosophy", "Philosophy of Mind",
                "Aesthetics", "Philosophy of Science", "Existentialism", "Eastern Philosophy"
            },
            // 55
            ["Political Science"] = new()
            {
                "International Relations", "Comparative Politics", "Public Policy",
                "Political Theory", "Electoral Systems", "Diplomacy",
                "Geopolitics", "Conflict Resolution", "Government & Governance", "Human Rights"
            },
            // 56
            ["Economics"] = new()
            {
                "Microeconomics", "Macroeconomics", "Development Economics",
                "Behavioral Economics", "International Trade", "Econometrics",
                "Public Finance", "Labor Economics", "Health Economics", "Environmental Economics"
            },
            // 57
            ["Sociology"] = new()
            {
                "Urban Sociology", "Rural Sociology", "Family Studies",
                "Social Stratification", "Cultural Studies", "Gender Studies",
                "Race & Ethnicity", "Criminology", "Demography", "Social Movements"
            },
            // 58
            ["Journalism"] = new()
            {
                "Investigative Journalism", "Broadcast Journalism", "Print Journalism",
                "Digital Journalism", "Photojournalism", "Data Journalism",
                "Sports Journalism", "Science Journalism", "Editorial Writing", "Fact-Checking"
            },
            // 59
            ["Tourism & Hospitality"] = new()
            {
                "Travel Planning", "Hotel Management", "Tour Guiding",
                "Event Tourism", "Sustainable Tourism", "Cruise Management",
                "Restaurant Operations", "Revenue Management", "Guest Experience", "Travel Writing"
            },
            // 60
            ["Interior Design"] = new()
            {
                "Residential Interior Design", "Commercial Interior Design", "Space Planning",
                "Furniture Design", "Lighting Design", "Color Theory",
                "3D Visualization", "Sustainable Interiors", "Kitchen & Bath Design", "Renovation Planning"
            },
            // 61
            ["Dentistry"] = new()
            {
                "General Dentistry", "Orthodontics", "Oral Surgery",
                "Periodontics", "Endodontics", "Prosthodontics",
                "Pediatric Dentistry", "Cosmetic Dentistry", "Dental Implants", "Dental Radiology"
            },
            // 62
            ["Veterinary Science"] = new()
            {
                "Small Animal Medicine", "Large Animal Medicine", "Veterinary Surgery",
                "Animal Nutrition", "Zoo Medicine", "Aquatic Animal Medicine",
                "Veterinary Pathology", "Animal Behavior", "Veterinary Dentistry", "Wildlife Conservation"
            },
            // 63
            ["Geoscience"] = new()
            {
                "Geology", "Geophysics", "Hydrogeology",
                "Petroleum Geology", "Geochemistry", "Remote Sensing",
                "Mineralogy", "Seismology", "Volcanology", "Glaciology"
            },
            // 64
            ["Robotics"] = new()
            {
                "Robot Programming", "Computer Vision for Robots", "Motion Planning",
                "Human-Robot Interaction", "Industrial Robotics", "Drone Technology",
                "Soft Robotics", "Robot Operating System", "Exoskeletons", "Swarm Robotics"
            },
            // 65
            ["Renewable Energy"] = new()
            {
                "Solar Energy", "Wind Energy", "Hydropower",
                "Biomass Energy", "Geothermal Energy", "Hydrogen Fuel",
                "Energy Storage", "Smart Grids", "Energy Policy", "Energy Auditing"
            },
            // 66
            ["Food Science"] = new()
            {
                "Food Chemistry", "Food Microbiology", "Food Processing",
                "Food Packaging", "Sensory Evaluation", "Quality Control",
                "Food Safety Regulations", "Nutraceuticals", "Fermentation Science", "Dairy Science"
            },
            // 67
            ["Archaeology"] = new()
            {
                "Field Excavation", "Archaeological Survey", "Artifact Analysis",
                "Dating Techniques", "Maritime Archaeology", "Digital Archaeology",
                "Ethnoarchaeology", "Historical Archaeology", "Geoarchaeology", "Museum Curation"
            },
            // 68
            ["Sports Science"] = new()
            {
                "Biomechanics", "Exercise Physiology", "Sports Nutrition",
                "Performance Analysis", "Injury Prevention", "Strength Testing",
                "Sports Analytics", "Talent Identification", "Recovery Science", "Sports Technology"
            },
            // 69
            ["Occupational Therapy"] = new()
            {
                "Pediatric OT", "Mental Health OT", "Neurological Rehabilitation",
                "Hand Therapy", "Ergonomics", "Assistive Technology",
                "Sensory Integration", "Work Rehabilitation", "Geriatric OT", "Community-Based OT"
            },
            // 70
            ["Speech Therapy"] = new()
            {
                "Articulation Disorders", "Language Disorders", "Fluency Disorders",
                "Voice Disorders", "Swallowing Disorders", "Autism & Communication",
                "Pediatric Speech Therapy", "Adult Rehabilitation", "AAC Devices", "Accent Modification"
            },
            // 71
            ["Financial Technology"] = new()
            {
                "Payment Systems", "Digital Banking", "RegTech",
                "InsurTech", "WealthTech", "Open Banking",
                "Buy Now Pay Later", "Robo-Advisors", "KYC & AML", "Embedded Finance"
            },
            // 72
            ["E-Commerce"] = new()
            {
                "Online Store Management", "Amazon Selling", "Dropshipping",
                "Marketplace Strategy", "Conversion Rate Optimization", "Product Listing Optimization",
                "Fulfillment by Amazon", "Shopify Development", "Customer Retention", "Returns Management"
            },
            // 73
            ["Construction"] = new()
            {
                "Site Management", "Quantity Surveying", "Structural Analysis",
                "MEP Engineering", "Construction Safety", "Contract Management",
                "BIM Modeling", "Material Estimation", "Green Building", "Project Scheduling"
            },
            // 74
            ["Petroleum Engineering"] = new()
            {
                "Reservoir Engineering", "Drilling Engineering", "Production Engineering",
                "Petroleum Geomechanics", "Well Logging", "EOR Techniques",
                "Pipeline Engineering", "Offshore Engineering", "LNG Processing", "HSE in Oil & Gas"
            },
            // 75
            ["Marine Science"] = new()
            {
                "Oceanography", "Marine Biology", "Fisheries Management",
                "Coral Reef Conservation", "Marine Ecology", "Hydrography",
                "Marine Geology", "Marine Pollution", "Aquaculture", "Deep Sea Research"
            },
            // 76
            ["Forensic Science"] = new()
            {
                "Crime Scene Investigation", "DNA Analysis", "Ballistics",
                "Digital Forensics", "Toxicology", "Fingerprint Analysis",
                "Forensic Accounting", "Document Examination", "Fire Investigation", "Forensic Entomology"
            },
            // 77
            ["Statistics"] = new()
            {
                "Descriptive Statistics", "Inferential Statistics", "Regression Analysis",
                "Bayesian Statistics", "Time Series Analysis", "Survival Analysis",
                "Multivariate Analysis", "Experimental Design", "Sampling Methods", "Statistical Software"
            },
            // 78
            ["Nanotechnology"] = new()
            {
                "Nanomaterials", "Nanoelectronics", "Nanomedicine",
                "Nano-Fabrication", "Carbon Nanotubes", "Quantum Dots",
                "Surface Science", "Self-Assembly", "Nano-Sensors", "Energy Harvesting"
            },
            // 79
            ["Nuclear Engineering"] = new()
            {
                "Reactor Design", "Radiation Protection", "Nuclear Fuel",
                "Waste Management", "Nuclear Safety", "Thermal Hydraulics",
                "Neutronics", "Nuclear Medicine", "Fusion Research", "Decommissioning"
            },
            // 80
            ["Material Science"] = new()
            {
                "Metals & Alloys", "Ceramics", "Polymers",
                "Composites", "Biomaterials", "Electronic Materials",
                "Nanomaterials", "Thin Film Technology", "Failure Analysis", "Corrosion Science"
            },
            // 81
            ["Cognitive Science"] = new()
            {
                "Cognitive Psychology", "Neuroscience", "Linguistics",
                "Human-Computer Interaction", "Perception & Attention", "Memory Research",
                "Decision Making", "Consciousness Studies", "Computational Modeling", "Behavioral Neuroscience"
            },
            // 82
            ["Disaster Management"] = new()
            {
                "Emergency Response Planning", "Search & Rescue", "Crisis Communication",
                "Relief Operations", "Disaster Risk Assessment", "Evacuation Planning",
                "Flood Management", "Earthquake Response", "Post-Disaster Recovery", "Humanitarian Aid"
            },
            // 83
            ["Mediation & Negotiation"] = new()
            {
                "Commercial Mediation", "Family Mediation", "Workplace Mediation",
                "International Negotiation", "Contract Negotiation", "Conflict De-escalation",
                "Arbitration", "Restorative Justice", "Cross-Cultural Negotiation", "Settlement Facilitation"
            },
            // 84
            ["Aviation"] = new()
            {
                "Fixed-Wing Flying", "Helicopter Flying", "Flight Instructing",
                "Aircraft Maintenance", "Avionics", "Flight Dispatching",
                "Cabin Crew Training", "Aviation Safety", "UAV & Drone Operation", "Air Charter Management"
            },
            // 85
            ["Urban Planning"] = new()
            {
                "Land Use Planning", "Transportation Planning", "Housing Policy",
                "Zoning & Regulations", "Smart Cities", "Community Engagement",
                "Environmental Planning", "GIS for Urban Planning", "Disaster Risk Reduction", "Heritage Conservation"
            },
            // 86
            ["Pharmacy"] = new()
            {
                "Clinical Pharmacy", "Pharmaceutical Research", "Drug Regulation",
                "Compounding", "Pharmacovigilance", "Hospital Pharmacy",
                "Retail Pharmacy", "Pharmacokinetics", "Drug Interactions", "Oncology Pharmacy"
            },
            // 87
            ["Logistics"] = new()
            {
                "Fleet Management", "Route Optimization", "Cold Chain Logistics",
                "Air Freight", "Sea Freight", "Ground Transportation",
                "Customs Clearance", "Third-Party Logistics", "Reverse Logistics", "Warehouse Automation"
            },
            // 88
            ["Theology"] = new()
            {
                "Islamic Studies", "Christian Theology", "Jewish Studies",
                "Comparative Religion", "Religious History", "Quranic Sciences",
                "Hadith Studies", "Biblical Studies", "Interfaith Dialogue", "Mysticism"
            },
            // 89
            ["Library Science"] = new()
            {
                "Cataloging", "Information Architecture", "Digital Libraries",
                "Archiving", "Reference Services", "Collection Development",
                "Knowledge Management", "Research Support", "Records Management", "Metadata Standards"
            },
            // 90
            ["Actuarial Science"] = new()
            {
                "Life Insurance", "General Insurance", "Pension Valuation",
                "Risk Modeling", "Mortality Tables", "Financial Mathematics",
                "Catastrophe Modeling", "Health Actuarial", "Reinsurance", "Regulatory Compliance"
            },
            // 91
            ["Textile Engineering"] = new()
            {
                "Fiber Science", "Yarn Manufacturing", "Fabric Production",
                "Dyeing & Finishing", "Technical Textiles", "Nonwoven Fabrics",
                "Textile Testing", "Smart Textiles", "Sustainable Textiles", "Apparel Engineering"
            },
            // 92
            ["Mining Engineering"] = new()
            {
                "Surface Mining", "Underground Mining", "Mine Planning",
                "Blasting & Explosives", "Mine Safety", "Mineral Processing",
                "Rock Mechanics", "Ventilation", "Geo-Mechanics", "Mine Surveying"
            },
            // 93
            ["Animal Science"] = new()
            {
                "Animal Nutrition", "Animal Breeding", "Poultry Science",
                "Equine Science", "Swine Production", "Beef Production",
                "Dairy Production", "Animal Welfare", "Aquaculture", "Animal Genetics"
            },
            // 94
            ["Horticulture"] = new()
            {
                "Landscape Horticulture", "Fruit Production", "Vegetable Production",
                "Floriculture", "Turfgrass Management", "Nursery Management",
                "Post-Harvest Handling", "Plant Pathology", "Irrigation Management", "Greenhouse Production"
            },
            // 95
            ["Sign Language"] = new()
            {
                "American Sign Language", "British Sign Language", "Arabic Sign Language",
                "Deaf Education", "Sign Language Interpreting", "Sign Language Linguistics",
                "Tactile Sign Language", "Cued Speech", "Deaf Culture", "Augmentative Communication"
            },
            // 96
            ["Air Traffic Management"] = new()
            {
                "Air Traffic Control", "Flight Planning", "Airspace Management",
                "Navigation Systems", "Communication Systems", "Safety Management",
                "Meteorology for Aviation", "Airport Operations", "Airline Operations", "Drone Traffic Management"
            },
            // 97
            ["Instructional Design"] = new()
            {
                "E-Learning Development", "LMS Administration", "Gamification",
                "Microlearning", "Blended Learning", "Assessment Design",
                "Learner Analytics", "Cognitive Load Theory", "Adult Learning", "SCORM Development"
            },
            // 98
            ["Optometry"] = new()
            {
                "Vision Testing", "Contact Lens Fitting", "Low Vision Rehabilitation",
                "Pediatric Optometry", "Geriatric Vision Care", "Binocular Vision",
                "Ocular Disease Management", "Sports Vision", "Neuro-Optometry", "Refractive Surgery Consulting"
            },
            // 99
            ["Customs & Trade"] = new()
            {
                "Import Procedures", "Export Procedures", "Customs Brokerage",
                "Trade Compliance", "Tariff Classification", "Free Trade Zones",
                "Letters of Credit", "Cargo Insurance", "Dangerous Goods Handling", "Incoterms"
            },
            // 100
            ["Biomedical Science"] = new()
            {
                "Hematology", "Microbiology", "Immunology",
                "Histopathology", "Clinical Biochemistry", "Medical Genetics",
                "Parasitology", "Virology", "Blood Banking", "Cytology"
            },
        };

        //foreach (var (skillName, subSkillNames) in data)
        //{
        //    var mainSkill = new MainSkill
        //    {
        //        Name = skillName,
        //        Slug = new Slugify.SlugHelper().GenerateSlug(skillName),
        //        IconKey = null,

        //        SubSkills = subSkillNames.Select(name => new SubSkill
        //        {
        //            Name = name,
        //            IconKey = null
        //        }).ToList()
        //    };

        //    await context.MainSkills.AddAsync(mainSkill);
        //}
        var mainSkills = new List<MainSkill>(data.Count);

        foreach (var (skillName, subSkillNames) in data)
        {
            var mainSkill = new MainSkill
            {
                Name = skillName,
                Slug = slugHelper.GenerateSlug(skillName),
                IconKey = null,
                SubSkills = subSkillNames.Select(subSkillName => new SubSkill
                {
                    Name = subSkillName,
                    IconKey = null
                }).ToList()
            };

            mainSkills.Add(mainSkill);
        }

        await context.MainSkills.AddRangeAsync(mainSkills);

        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────
    //  Languages SEEDER
    // ─────────────────────────────────────────────────────────
    private static async Task SeedLanguagesAsync(AppDbContext context)
    {
        //if (await context.Languages.AnyAsync()) return;   // guard — run once
        var languages = new List<Language>
    {
        new() { Name = "Abkhazian",              Code = "ab" },
        new() { Name = "Afar",                   Code = "aa" },
        new() { Name = "Afrikaans",              Code = "af" },
        new() { Name = "Akan",                   Code = "ak" },
        new() { Name = "Albanian",               Code = "sq" },
        new() { Name = "Amharic",                Code = "am" },
        new() { Name = "Arabic",                 Code = "ar" },
        new() { Name = "Aragonese",              Code = "an" },
        new() { Name = "Armenian",               Code = "hy" },
        new() { Name = "Assamese",               Code = "as" },
        new() { Name = "Avaric",                 Code = "av" },
        new() { Name = "Avestan",                Code = "ae" },
        new() { Name = "Aymara",                 Code = "ay" },
        new() { Name = "Azerbaijani",            Code = "az" },
        new() { Name = "Bambara",                Code = "bm" },
        new() { Name = "Bashkir",                Code = "ba" },
        new() { Name = "Basque",                 Code = "eu" },
        new() { Name = "Belarusian",             Code = "be" },
        new() { Name = "Bengali",                Code = "bn" },
        new() { Name = "Bihari",                 Code = "bh" },
        new() { Name = "Bislama",                Code = "bi" },
        new() { Name = "Bosnian",                Code = "bs" },
        new() { Name = "Breton",                 Code = "br" },
        new() { Name = "Bulgarian",              Code = "bg" },
        new() { Name = "Burmese",                Code = "my" },
        new() { Name = "Catalan",                Code = "ca" },
        new() { Name = "Chamorro",               Code = "ch" },
        new() { Name = "Chechen",                Code = "ce" },
        new() { Name = "Chichewa",               Code = "ny" },
        new() { Name = "Chinese (Simplified)",   Code = "zh-Hans" },
        new() { Name = "Chinese (Traditional)",  Code = "zh-Hant" },
        new() { Name = "Chuvash",                Code = "cv" },
        new() { Name = "Cornish",                Code = "kw" },
        new() { Name = "Corsican",               Code = "co" },
        new() { Name = "Cree",                   Code = "cr" },
        new() { Name = "Croatian",               Code = "hr" },
        new() { Name = "Czech",                  Code = "cs" },
        new() { Name = "Danish",                 Code = "da" },
        new() { Name = "Divehi",                 Code = "dv" },
        new() { Name = "Dutch",                  Code = "nl" },
        new() { Name = "Dzongkha",               Code = "dz" },
        new() { Name = "English",                Code = "en" },
        new() { Name = "Esperanto",              Code = "eo" },
        new() { Name = "Estonian",               Code = "et" },
        new() { Name = "Ewe",                    Code = "ee" },
        new() { Name = "Faroese",                Code = "fo" },
        new() { Name = "Fijian",                 Code = "fj" },
        new() { Name = "Finnish",                Code = "fi" },
        new() { Name = "French",                 Code = "fr" },
        new() { Name = "Fula",                   Code = "ff" },
        new() { Name = "Galician",               Code = "gl" },
        new() { Name = "Georgian",               Code = "ka" },
        new() { Name = "German",                 Code = "de" },
        new() { Name = "Greek",                  Code = "el" },
        new() { Name = "Guaraní",                Code = "gn" },
        new() { Name = "Gujarati",               Code = "gu" },
        new() { Name = "Haitian Creole",         Code = "ht" },
        new() { Name = "Hausa",                  Code = "ha" },
        new() { Name = "Hebrew",                 Code = "he" },
        new() { Name = "Herero",                 Code = "hz" },
        new() { Name = "Hindi",                  Code = "hi" },
        new() { Name = "Hiri Motu",              Code = "ho" },
        new() { Name = "Hungarian",              Code = "hu" },
        new() { Name = "Interlingua",            Code = "ia" },
        new() { Name = "Indonesian",             Code = "id" },
        new() { Name = "Interlingue",            Code = "ie" },
        new() { Name = "Irish",                  Code = "ga" },
        new() { Name = "Igbo",                   Code = "ig" },
        new() { Name = "Inupiaq",                Code = "ik" },
        new() { Name = "Ido",                    Code = "io" },
        new() { Name = "Icelandic",              Code = "is" },
        new() { Name = "Italian",                Code = "it" },
        new() { Name = "Inuktitut",              Code = "iu" },
        new() { Name = "Japanese",               Code = "ja" },
        new() { Name = "Javanese",               Code = "jv" },
        new() { Name = "Kalaallisut",            Code = "kl" },
        new() { Name = "Kannada",                Code = "kn" },
        new() { Name = "Kanuri",                 Code = "kr" },
        new() { Name = "Kashmiri",               Code = "ks" },
        new() { Name = "Kazakh",                 Code = "kk" },
        new() { Name = "Khmer",                  Code = "km" },
        new() { Name = "Kikuyu",                 Code = "ki" },
        new() { Name = "Kinyarwanda",            Code = "rw" },
        new() { Name = "Kyrgyz",                 Code = "ky" },
        new() { Name = "Komi",                   Code = "kv" },
        new() { Name = "Kongo",                  Code = "kg" },
        new() { Name = "Korean",                 Code = "ko" },
        new() { Name = "Kurdish",                Code = "ku" },
        new() { Name = "Kwanyama",               Code = "kj" },
        new() { Name = "Latin",                  Code = "la" },
        new() { Name = "Luxembourgish",          Code = "lb" },
        new() { Name = "Luganda",                Code = "lg" },
        new() { Name = "Limburgish",             Code = "li" },
        new() { Name = "Lingala",                Code = "ln" },
        new() { Name = "Lao",                    Code = "lo" },
        new() { Name = "Lithuanian",             Code = "lt" },
        new() { Name = "Luba-Katanga",           Code = "lu" },
        new() { Name = "Latvian",                Code = "lv" },
        new() { Name = "Manx",                   Code = "gv" },
        new() { Name = "Macedonian",             Code = "mk" },
        new() { Name = "Malagasy",               Code = "mg" },
        new() { Name = "Malay",                  Code = "ms" },
        new() { Name = "Malayalam",              Code = "ml" },
        new() { Name = "Maltese",                Code = "mt" },
        new() { Name = "Māori",                  Code = "mi" },
        new() { Name = "Marathi",                Code = "mr" },
        new() { Name = "Marshallese",            Code = "mh" },
        new() { Name = "Mongolian",              Code = "mn" },
        new() { Name = "Nauru",                  Code = "na" },
        new() { Name = "Navajo",                 Code = "nv" },
        new() { Name = "Norwegian Bokmål",       Code = "nb" },
        new() { Name = "North Ndebele",          Code = "nd" },
        new() { Name = "Nepali",                 Code = "ne" },
        new() { Name = "Ndonga",                 Code = "ng" },
        new() { Name = "Norwegian Nynorsk",      Code = "nn" },
        new() { Name = "Norwegian",              Code = "no" },
        new() { Name = "Nuosu",                  Code = "ii" },
        new() { Name = "South Ndebele",          Code = "nr" },
        new() { Name = "Occitan",                Code = "oc" },
        new() { Name = "Ojibwe",                 Code = "oj" },
        new() { Name = "Old Church Slavonic",    Code = "cu" },
        new() { Name = "Oromo",                  Code = "om" },
        new() { Name = "Oriya",                  Code = "or" },
        new() { Name = "Ossetian",               Code = "os" },
        new() { Name = "Punjabi",                Code = "pa" },
        new() { Name = "Pāli",                   Code = "pi" },
        new() { Name = "Persian",                Code = "fa" },
        new() { Name = "Polish",                 Code = "pl" },
        new() { Name = "Pashto",                 Code = "ps" },
        new() { Name = "Portuguese",             Code = "pt" },
        new() { Name = "Quechua",                Code = "qu" },
        new() { Name = "Romansh",                Code = "rm" },
        new() { Name = "Kirundi",                Code = "rn" },
        new() { Name = "Romanian",               Code = "ro" },
        new() { Name = "Russian",                Code = "ru" },
        new() { Name = "Sanskrit",               Code = "sa" },
        new() { Name = "Sardinian",              Code = "sc" },
        new() { Name = "Sindhi",                 Code = "sd" },
        new() { Name = "Northern Sami",          Code = "se" },
        new() { Name = "Samoan",                 Code = "sm" },
        new() { Name = "Sango",                  Code = "sg" },
        new() { Name = "Serbian",                Code = "sr" },
        new() { Name = "Scottish Gaelic",        Code = "gd" },
        new() { Name = "Shona",                  Code = "sn" },
        new() { Name = "Sinhala",                Code = "si" },
        new() { Name = "Slovak",                 Code = "sk" },
        new() { Name = "Slovenian",              Code = "sl" },
        new() { Name = "Somali",                 Code = "so" },
        new() { Name = "Southern Sotho",         Code = "st" },
        new() { Name = "Spanish",                Code = "es" },
        new() { Name = "Sundanese",              Code = "su" },
        new() { Name = "Swahili",                Code = "sw" },
        new() { Name = "Swati",                  Code = "ss" },
        new() { Name = "Swedish",                Code = "sv" },
        new() { Name = "Tamil",                  Code = "ta" },
        new() { Name = "Telugu",                 Code = "te" },
        new() { Name = "Tajik",                  Code = "tg" },
        new() { Name = "Thai",                   Code = "th" },
        new() { Name = "Tigrinya",               Code = "ti" },
        new() { Name = "Tibetan",                Code = "bo" },
        new() { Name = "Turkmen",                Code = "tk" },
        new() { Name = "Tagalog",                Code = "tl" },
        new() { Name = "Tswana",                 Code = "tn" },
        new() { Name = "Tonga",                  Code = "to" },
        new() { Name = "Turkish",                Code = "tr" },
        new() { Name = "Tsonga",                 Code = "ts" },
        new() { Name = "Tatar",                  Code = "tt" },
        new() { Name = "Twi",                    Code = "tw" },
        new() { Name = "Tahitian",               Code = "ty" },
        new() { Name = "Uyghur",                 Code = "ug" },
        new() { Name = "Ukrainian",              Code = "uk" },
        new() { Name = "Urdu",                   Code = "ur" },
        new() { Name = "Uzbek",                  Code = "uz" },
        new() { Name = "Venda",                  Code = "ve" },
        new() { Name = "Vietnamese",             Code = "vi" },
        new() { Name = "Volapük",                Code = "vo" },
        new() { Name = "Walloon",                Code = "wa" },
        new() { Name = "Welsh",                  Code = "cy" },
        new() { Name = "Wolof",                  Code = "wo" },
        new() { Name = "Western Frisian",        Code = "fy" },
        new() { Name = "Xhosa",                  Code = "xh" },
        new() { Name = "Yiddish",                Code = "yi" },
        new() { Name = "Yoruba",                 Code = "yo" },
        new() { Name = "Zhuang",                 Code = "za" },
        new() { Name = "Zulu",                   Code = "zu" },
    };
        foreach (var language in languages)
        {
            var exists = await context.Languages
                .AsNoTracking()
                .AnyAsync(x => x.Code == language.Code);

            if (exists)
                continue;

            await context.Languages.AddAsync(language);
        }
        //await context.Languages.AddRangeAsync(languages);
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────
    //  Badges SEEDER
    // ─────────────────────────────────────────────────────────
    private static async Task SeedBadgesAsync(AppDbContext context)
    {
        //if (await context.Badges.AnyAsync()) return;

        var badges = new List<Badge>
            {
                // Session Count Badges
                new Badge
                {
                    Name = "First Exchange",
                    Slug = "first-exchange",
                    Description = "Completed your first skill exchange session.",
                    IconKey = "badge_first_exchange",
                    CriteriaType = BadgeCriteriaType.SessionCount,
                    CriteriaThreshold = 1
                },

                new Badge
                {
                    Name = "Helpful Mentor",
                    Slug = "helpful-mentor",
                    Description = "Completed 10 skill exchange sessions.",
                    IconKey = "badge_helpful_mentor",
                    CriteriaType = BadgeCriteriaType.SessionCount,
                    CriteriaThreshold = 10
                },

                new Badge
                {
                    Name = "Skill Master",
                    Slug = "skill-master",
                    Description = "Completed 50 skill exchange sessions.",
                    IconKey = "badge_skill_master",
                    CriteriaType = BadgeCriteriaType.SessionCount,
                    CriteriaThreshold = 50
                },

                // Rating Badges
                new Badge
                {
                    Name = "Top Rated",
                    Slug = "top-rated",
                    Description = "Maintained an average rating of 4.5 or higher.",
                    IconKey = "badge_top_rated",
                    CriteriaType = BadgeCriteriaType.AverageRating,
                    CriteriaThreshold = 45 // 4.5 * 10
                },

                new Badge
                {
                    Name = "Community Favorite",
                    Slug = "community-favorite",
                    Description = "Maintained an average rating of 5.0.",
                    IconKey = "badge_community_favorite",
                    CriteriaType = BadgeCriteriaType.AverageRating,
                    CriteriaThreshold = 50 // 5.0 * 10
                },

                // Consistent Helping
                new Badge
                {
                    Name = "Consistent Helper",
                    Slug = "consistent-helper",
                    Description = "Helped other users consistently over time.",
                    IconKey = "badge_consistent_helper",
                    CriteriaType = BadgeCriteriaType.ConsistentHelping,
                    CriteriaThreshold = 30
                }
            };
        foreach (var badge in badges)
        {
            var exists = await context.Badges
                .AsNoTracking()
                .AnyAsync(x => x.Slug == badge.Slug);

            if (exists)
                continue;

            await context.Badges.AddAsync(badge);
        }
        //await context.Badges.AddRangeAsync(badges);
        await context.SaveChangesAsync();
    }

}