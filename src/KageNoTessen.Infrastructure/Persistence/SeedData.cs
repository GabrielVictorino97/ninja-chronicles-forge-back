using Microsoft.EntityFrameworkCore;
using KageNoTessen.Domain;

namespace KageNoTessen.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db, string adminEmail, string adminPassword)
    {
        if (await db.Users.AnyAsync()) return;

        // Admin user
        var admin = User.Create(adminEmail, "Admin Hokage", BCrypt.Net.BCrypt.HashPassword(adminPassword));
        admin.SetRole(UserRole.SuperAdmin);
        db.Users.Add(admin);

        // Villages
        var villages = new[]
        {
            Village.Create("Konoha", "Konohagakure", "País do Fogo",
                "A Vila Oculta da Folha, lar dos Hokages.", "🍃", "#FF6B35"),
            Village.Create("Suna", "Sunagakure", "País do Vento",
                "A Vila Oculta da Areia, governada pelo Kazekage.", "⏳", "#D4A574"),
            Village.Create("Kiri", "Kirigakure", "País da Água",
                "A Vila Oculta da Névoa, terra dos 7 Espadachins.", "🌊", "#4ECDC4"),
            Village.Create("Kumo", "Kumogakure", "País do Raio",
                "A Vila Oculta da Nuvem, conhecida por seu poder militar.", "⚡", "#FFE66D"),
            Village.Create("Iwa", "Iwagakure", "País da Terra",
                "A Vila Oculta da Pedra, famosa por sua defesa impenetrável.", "🪨", "#C4A484"),
            Village.Create("Ame", "Amegakure", "País da Chuva",
                "A Vila Oculta da Chuva, terra de Pain e Konan.", "🌧️", "#7B68EE"),
            Village.Create("Oto", "Otogakure", "País do Som",
                "A Vila Oculta do Som, fundada por Orochimaru.", "🎵", "#9B59B6"),
        };
        db.Villages.AddRange(villages);

        // Bloodline Clans
        var clans = new[]
        {
            BloodlineClan.Create("Uchiha", "Clã do Sharingan, poderoso doujutsu.", "Sharingan: +10% crit chance", "🔴"),
            BloodlineClan.Create("Hyuga", "Clã do Byakugan, visão 360°.", "Byakugan: +15% precision", "👁️"),
            BloodlineClan.Create("Uzumaki", "Clã de chakra abundante e selamento.", "Chakra: +20% chakra max", "🌀"),
            BloodlineClan.Create("Senju", "Clã da floresta, ancestral de Konoha.", "Vitality: +15% hp max", "🌳"),
            BloodlineClan.Create("Nara", "Clã das sombras, estrategistas natos.", "Intelligence: +10% mental resistance", "🌑"),
            BloodlineClan.Create("Akimichi", "Clã da expansão corporal.", "Vitality: +10% defense", "🍖"),
            BloodlineClan.Create("Yamanaka", "Clã das técnicas mentais.", "Mind: +10% genjutsu attack", "💐"),
            BloodlineClan.Create("Aburame", "Clã dos insetos parasitas.", "Insects: +5% dodge", "🪲"),
            BloodlineClan.Create("Inuzuka", "Clã dos ninjas feras com cães.", "Beast: +10% physical attack", "🐺"),
            BloodlineClan.Create("Sarutobi", "Clã do fogo e da força de vontade.", "Will: +10% fire damage", "🔥"),
            BloodlineClan.Create("Kaguya", "Clã dos ossos, manipulação óssea.", "Bone: +10% physical defense", "💀"),
            BloodlineClan.Create("Hozuki", "Clã da hidratação, corpos líquidos.", "Water: +10% water damage", "💧"),
            BloodlineClan.Create("Hatake", "Clã do Raio, reflexos velozes.", "Lightning: +10% initiative", "⚡"),
            BloodlineClan.Create("Sabaku", "Clã da areia e magnetismo.", "Sand: +10% defense", "🏜️"),
        };
        foreach (var c in clans) { c.VillageOrigin = "Konoha"; }
        db.BloodlineClans.AddRange(clans);

        // Jutsus
        var jutsus = new[]
        {
            (Name: "Bunshin no Jutsu", Type: JutsuType.Ninjutsu, Chakra: 10, Cd: 1, Dmg: 0, Desc: "Técnica de clone ilusório.", Elem: (ElementAffinity?)null),
            (Name: "Henge no Jutsu", Type: JutsuType.Ninjutsu, Chakra: 10, Cd: 1, Dmg: 0, Desc: "Técnica de transformação.", Elem: (ElementAffinity?)null),
            (Name: "Kawarimi no Jutsu", Type: JutsuType.Ninjutsu, Chakra: 15, Cd: 3, Dmg: 0, Desc: "Técnica de substituição corporal.", Elem: (ElementAffinity?)null),
            (Name: "Kunai Throw", Type: JutsuType.Taijutsu, Chakra: 0, Cd: 1, Dmg: 30, Desc: "Lançamento de kunai com precisão.", Elem: (ElementAffinity?)null),
            (Name: "Basic Taijutsu Combo", Type: JutsuType.Taijutsu, Chakra: 0, Cd: 1, Dmg: 40, Desc: "Combo básico de golpes corpo a corpo.", Elem: (ElementAffinity?)null),
            (Name: "Chakra Control", Type: JutsuType.Ninjutsu, Chakra: 5, Cd: 1, Dmg: 20, Desc: "Controle básico de chakra para ataque.", Elem: (ElementAffinity?)null),
            (Name: "Rasengan", Type: JutsuType.Ninjutsu, Chakra: 40, Cd: 4, Dmg: 100, Desc: "Esfera giratória de chakra puro.", Elem: (ElementAffinity?)null),
            (Name: "Chidori", Type: JutsuType.Ninjutsu, Chakra: 45, Cd: 4, Dmg: 110, Desc: "Lâmina de raio concentrada na mão.", Elem: ElementAffinity.Raiton),
            (Name: "Kage Bunshin no Jutsu", Type: JutsuType.Kinjutsu, Chakra: 50, Cd: 6, Dmg: 80, Desc: "Multiplicação de clones sólidos.", Elem: (ElementAffinity?)null),
            (Name: "Katon: Goukakyuu no Jutsu", Type: JutsuType.KekkeiGenkai, Chakra: 35, Cd: 3, Dmg: 90, Desc: "Bola de fogo massiva.", Elem: ElementAffinity.Katon),
            (Name: "Suiton: Suiryuudan no Jutsu", Type: JutsuType.Ninjutsu, Chakra: 35, Cd: 3, Dmg: 85, Desc: "Dragão de água ofensivo.", Elem: ElementAffinity.Suiton),
            (Name: "Doton: Doryuuheki", Type: JutsuType.Ninjutsu, Chakra: 30, Cd: 3, Dmg: 50, Desc: "Muralha de terra defensiva.", Elem: ElementAffinity.Doton),
            (Name: "Fuuton: Rasenshuriken", Type: JutsuType.Kinjutsu, Chakra: 60, Cd: 8, Dmg: 150, Desc: "Rasengan com elemento vento.", Elem: ElementAffinity.Fuuton),
            (Name: "Raiton: Raikiri", Type: JutsuType.Ninjutsu, Chakra: 50, Cd: 5, Dmg: 130, Desc: "Lâmina de relâmpago.", Elem: ElementAffinity.Raiton),
            (Name: "Jyuuken", Type: JutsuType.Taijutsu, Chakra: 20, Cd: 2, Dmg: 60, Desc: "Punho gentil do clã Hyuga.", Elem: (ElementAffinity?)null),
            (Name: "Kaiten", Type: JutsuType.Taijutsu, Chakra: 25, Cd: 3, Dmg: 55, Desc: "Rotação defensiva do Hyuga.", Elem: (ElementAffinity?)null),
            (Name: "Kagemane no Jutsu", Type: JutsuType.Ninjutsu, Chakra: 25, Cd: 3, Dmg: 45, Desc: "Técnica de aprisionamento de sombra.", Elem: (ElementAffinity?)null),
            (Name: "Baika no Jutsu", Type: JutsuType.Ninjutsu, Chakra: 30, Cd: 4, Dmg: 70, Desc: "Expansão do tamanho corporal.", Elem: (ElementAffinity?)null),
            (Name: "Shintenshin no Jutsu", Type: JutsuType.Ninjutsu, Chakra: 35, Cd: 5, Dmg: 50, Desc: "Transferência de mente.", Elem: (ElementAffinity?)null),
            (Name: "Gatsuuga", Type: JutsuType.Taijutsu, Chakra: 15, Cd: 2, Dmg: 65, Desc: "Ataque giratório canino.", Elem: (ElementAffinity?)null),
            (Name: "Sabaku Kyuu", Type: JutsuType.Ninjutsu, Chakra: 30, Cd: 3, Dmg: 75, Desc: "Aprisionamento de areia.", Elem: ElementAffinity.Doton),
            (Name: "Amaterasu", Type: JutsuType.KekkeiGenkai, Chakra: 70, Cd: 10, Dmg: 180, Desc: "Chamas negras inextinguíveis.", Elem: ElementAffinity.Katon),
            (Name: "Tsukuyomi", Type: JutsuType.Genjutsu, Chakra: 60, Cd: 8, Dmg: 140, Desc: "Genjutsu supremo do Sharingan.", Elem: (ElementAffinity?)null),
            (Name: "Susanoo", Type: JutsuType.KekkeiGenkai, Chakra: 100, Cd: 15, Dmg: 200, Desc: "Avatar etéreo do guerreiro.", Elem: (ElementAffinity?)null),
            // Novos jutsus — Katon
            (Name: "Katon: Housenka no Jutsu", Type: JutsuType.Ninjutsu, Chakra: 25, Cd: 2, Dmg: 60, Desc: "Múltiplas bolas de fogo lançadas em sequência.", Elem: ElementAffinity.Katon),
            (Name: "Katon: Karyuu Endan", Type: JutsuType.Ninjutsu, Chakra: 55, Cd: 5, Dmg: 120, Desc: "Jato de fogo em forma de dragão flamejante.", Elem: ElementAffinity.Katon),
            // Suiton
            (Name: "Suiton: Suijinheki", Type: JutsuType.Ninjutsu, Chakra: 30, Cd: 3, Dmg: 20, Desc: "Parede de água defensiva que bloqueia ataques.", Elem: ElementAffinity.Suiton),
            (Name: "Suiton: Daibakufu no Jutsu", Type: JutsuType.Ninjutsu, Chakra: 50, Cd: 5, Dmg: 110, Desc: "Explosão massiva de água em todas as direções.", Elem: ElementAffinity.Suiton),
            (Name: "Mizu Bunshin no Jutsu", Type: JutsuType.Ninjutsu, Chakra: 20, Cd: 2, Dmg: 0, Desc: "Clone de água com 10% do poder do usuário.", Elem: ElementAffinity.Suiton),
            // Doton
            (Name: "Doton: Shinjuu Zanshu no Jutsu", Type: JutsuType.Ninjutsu, Chakra: 25, Cd: 3, Dmg: 55, Desc: "Afundar no solo e atacar pelas costas do inimigo.", Elem: ElementAffinity.Doton),
            (Name: "Doton: Doryuudan", Type: JutsuType.Ninjutsu, Chakra: 45, Cd: 4, Dmg: 95, Desc: "Projétil de terra em forma de dragão.", Elem: ElementAffinity.Doton),
            (Name: "Doton: Yomi Numa", Type: JutsuType.Ninjutsu, Chakra: 40, Cd: 5, Dmg: 30, Desc: "Pântano de lama que prende e afunda o inimigo.", Elem: ElementAffinity.Doton),
            // Fuuton
            (Name: "Fuuton: Daitoppa", Type: JutsuType.Ninjutsu, Chakra: 30, Cd: 3, Dmg: 65, Desc: "Rajada de vento cortante que empurra o inimigo.", Elem: ElementAffinity.Fuuton),
            (Name: "Fuuton: Kazekiri", Type: JutsuType.Ninjutsu, Chakra: 40, Cd: 3, Dmg: 80, Desc: "Lâmina de vento comprimido de alto poder de corte.", Elem: ElementAffinity.Fuuton),
            (Name: "Fuuton: Atsugai", Type: JutsuType.Ninjutsu, Chakra: 55, Cd: 5, Dmg: 130, Desc: "Massiva rajada de vento comprimido em área.", Elem: ElementAffinity.Fuuton),
            // Raiton
            (Name: "Raiton: Gian", Type: JutsuType.Ninjutsu, Chakra: 35, Cd: 3, Dmg: 75, Desc: "Ondas de eletricidade em forma de lanças.", Elem: ElementAffinity.Raiton),
            (Name: "Chidori Nagashi", Type: JutsuType.Ninjutsu, Chakra: 45, Cd: 4, Dmg: 100, Desc: "Descarga elétrica em área ao redor do corpo.", Elem: ElementAffinity.Raiton),
            (Name: "Kirin", Type: JutsuType.Kinjutsu, Chakra: 80, Cd: 10, Dmg: 190, Desc: "Raio divino invocado das nuvens, devastador.", Elem: ElementAffinity.Raiton),
            // Sem elemento — Taijutsu / Bukijutsu
            (Name: "Shuriken Kage Bunshin no Jutsu", Type: JutsuType.Ninjutsu, Chakra: 30, Cd: 3, Dmg: 70, Desc: "Multiplica uma shuriken em milhares.", Elem: (ElementAffinity?)null),
            (Name: "Soufuushasan no Tachi", Type: JutsuType.Taijutsu, Chakra: 20, Cd: 2, Dmg: 55, Desc: "Ataque giratório com katana de múltiplos ângulos.", Elem: (ElementAffinity?)null),
            (Name: "Hakke Rokujuuyonshou", Type: JutsuType.Taijutsu, Chakra: 40, Cd: 5, Dmg: 120, Desc: "64 palmas do Hyuga, fecha os tenketsus.", Elem: (ElementAffinity?)null),
            (Name: "Ura Renge", Type: JutsuType.Taijutsu, Chakra: 0, Cd: 8, Dmg: 160, Desc: "Combo de taijutsu proibido dos portões internos.", Elem: (ElementAffinity?)null),
            // Genjutsu
            (Name: "Magen: Narakumi no Jutsu", Type: JutsuType.Genjutsu, Chakra: 25, Cd: 3, Dmg: 40, Desc: "Ilusão demoníaca, mostra o pior medo do alvo.", Elem: (ElementAffinity?)null),
            (Name: "Kokuangyou no Jutsu", Type: JutsuType.Genjutsu, Chakra: 50, Cd: 6, Dmg: 80, Desc: "Ilusão que cega o alvo na escuridão total.", Elem: (ElementAffinity?)null),
            // Iryo Ninjutsu
            (Name: "Shousen no Jutsu", Type: JutsuType.IryoNinjutsu, Chakra: 30, Cd: 2, Dmg: 0, Desc: "Técnica de cura mística das mãos verdes.", Elem: (ElementAffinity?)null),
            (Name: "Chakra no Mesu", Type: JutsuType.IryoNinjutsu, Chakra: 25, Cd: 3, Dmg: 45, Desc: "Bisturi de chakra, corta sem deixar cicatriz.", Elem: (ElementAffinity?)null),
            // Kuchiyose
            (Name: "Kuchiyose: Gamabunta", Type: JutsuType.Kuchiyose, Chakra: 60, Cd: 10, Dmg: 140, Desc: "Invoca o chefe sapo Gamabunta.", Elem: (ElementAffinity?)null),
            (Name: "Kuchiyose: Manda", Type: JutsuType.Kuchiyose, Chakra: 65, Cd: 10, Dmg: 150, Desc: "Invoca a serpente gigante Manda.", Elem: (ElementAffinity?)null),
            // Fuinjutsu
            (Name: "Fuinjutsu: Shiki Fuujin", Type: JutsuType.Fuinjutsu, Chakra: 90, Cd: 20, Dmg: 250, Desc: "Selo do Deus da Morte, sacrifica o usuário.", Elem: (ElementAffinity?)null),
        };
        foreach (var (name, type, chakra, cd, dmg, desc, elem) in jutsus)
        {
            var j = Jutsu.Create(name, type, chakra, cd, dmg, desc);
            j.Element = elem;
            if (chakra >= 40) j.MinLevel = 10;
            if (chakra >= 60) j.MinLevel = 20;
            if (chakra >= 100) j.MinLevel = 40;
            db.Jutsus.Add(j);
        }

        // Missions
        var missions = new[]
        {
            (Title: "Entrega de suprimentos", Rank: Rank.D, Energy: 10, Xp: 30, Ryous: 50, Dur: 1, Type: MissionType.Delivery),
            (Title: "Patrulha da vila", Rank: Rank.D, Energy: 15, Xp: 40, Ryous: 60, Dur: 1, Type: MissionType.Patrol),
            (Title: "Capturar o gato Tora", Rank: Rank.D, Energy: 10, Xp: 25, Ryous: 40, Dur: 2, Type: MissionType.Capture),
            (Title: "Escolta de comerciante", Rank: Rank.C, Energy: 20, Xp: 80, Ryous: 120, Dur: 5, Type: MissionType.Escort),
            (Title: "Investigar sumiço", Rank: Rank.C, Energy: 25, Xp: 90, Ryous: 150, Dur: 5, Type: MissionType.Investigation),
            (Title: "Defender a vila", Rank: Rank.B, Energy: 35, Xp: 150, Ryous: 300, Dur: 15, Type: MissionType.VillageDefense),
            (Title: "Capturar ninja renegado", Rank: Rank.B, Energy: 40, Xp: 200, Ryous: 400, Dur: 15, Type: MissionType.Capture),
            (Title: "Infiltração na base inimiga", Rank: Rank.A, Energy: 50, Xp: 350, Ryous: 800, Dur: 30, Type: MissionType.Infiltration),
            (Title: "Resgate de reféns", Rank: Rank.A, Energy: 55, Xp: 400, Ryous: 1000, Dur: 30, Type: MissionType.Rescue),
            (Title: "Derrotar o Jinchuuriki", Rank: Rank.S, Energy: 80, Xp: 800, Ryous: 3000, Dur: 60, Type: MissionType.Boss),
            // Novas missões
            (Title: "Limpar o campo de treinamento", Rank: Rank.D, Energy: 10, Xp: 25, Ryous: 40, Dur: 1, Type: MissionType.Training),
            (Title: "Achar o gato desaparecido", Rank: Rank.D, Energy: 5, Xp: 20, Ryous: 30, Dur: 2, Type: MissionType.Investigation),
            (Title: "Proteger a caravana mercante", Rank: Rank.C, Energy: 25, Xp: 100, Ryous: 180, Dur: 5, Type: MissionType.Escort),
            (Title: "Espionar acampamento inimigo", Rank: Rank.C, Energy: 20, Xp: 85, Ryous: 140, Dur: 8, Type: MissionType.Investigation),
            (Title: "Destruir ponte estratégica", Rank: Rank.B, Energy: 35, Xp: 180, Ryous: 350, Dur: 15, Type: MissionType.Infiltration),
            (Title: "Capturar espião da vila rival", Rank: Rank.B, Energy: 30, Xp: 160, Ryous: 320, Dur: 20, Type: MissionType.Capture),
            (Title: "Assassinar lorde feudal corrupto", Rank: Rank.A, Energy: 50, Xp: 380, Ryous: 900, Dur: 30, Type: MissionType.Assassination),
            (Title: "Recuperar pergaminho proibido", Rank: Rank.A, Energy: 45, Xp: 420, Ryous: 1100, Dur: 45, Type: MissionType.Infiltration),
            (Title: "Derrotar membro da Akatsuki", Rank: Rank.S, Energy: 75, Xp: 900, Ryous: 3500, Dur: 60, Type: MissionType.Boss),
            (Title: "Proteger o Kage da aldeia", Rank: Rank.S, Energy: 90, Xp: 1000, Ryous: 5000, Dur: 90, Type: MissionType.VillageDefense),
            // Missões de longa duração por rank (Issue 10)
            (Title: "Patrulha estendida da fronteira", Rank: Rank.D, Energy: 30, Xp: 200, Ryous: 300, Dur: 60, Type: MissionType.Patrol),
            (Title: "Guarda noturna da vila", Rank: Rank.C, Energy: 40, Xp: 500, Ryous: 600, Dur: 180, Type: MissionType.VillageDefense),
            (Title: "Caçada a ninjas renegados B-rank", Rank: Rank.B, Energy: 60, Xp: 1000, Ryous: 1500, Dur: 360, Type: MissionType.Capture),
            (Title: "Infiltração prolongada na vila inimiga", Rank: Rank.A, Energy: 80, Xp: 2000, Ryous: 3000, Dur: 600, Type: MissionType.Infiltration),
            (Title: "Defender o ataque de Bijuu", Rank: Rank.S, Energy: 100, Xp: 5000, Ryous: 8000, Dur: 1200, Type: MissionType.Boss),
        };
        foreach (var (title, rank, energy, xp, ryous, dur, type) in missions)
        {
            var m = Mission.Create(title, rank, energy, xp, ryous);
            m.Description = $"{title} — Missão Rank {rank}.";
            m.Type = type;
            m.DurationMinutes = dur;
            if (rank >= Rank.B) { m.MinLevel = 10; m.MinGraduation = Graduation.Chunin; }
            if (rank >= Rank.A) { m.MinLevel = 25; m.MinGraduation = Graduation.Jounin; }
            if (rank >= Rank.S) { m.MinLevel = 40; m.MinGraduation = Graduation.ANBU; }
            m.Drops = rank >= Rank.B ? new[] { "Kunai especial", "Pergaminho" } : new[] { "Kunai" };
            db.Missions.Add(m);
        }

        // Items
        var items = new (string Name, ItemType Type, ItemRarity Rarity, int Price, string Icon,
            int Atk, int Def, int Int, int Agi, int Vit, int Chk, int Luk)[]
        {
            // Weapons — foco em ataque e agilidade
            ("Kunai", ItemType.Weapon, ItemRarity.Common, 50, "knife",
                Atk: 5, Def: 0, Int: 0, Agi: 2, Vit: 0, Chk: 0, Luk: 0),
            ("Shuriken", ItemType.Weapon, ItemRarity.Common, 40, "disc-3",
                Atk: 3, Def: 0, Int: 0, Agi: 3, Vit: 0, Chk: 0, Luk: 1),
            ("Senbon", ItemType.Weapon, ItemRarity.Common, 25, "pin",
                Atk: 2, Def: 0, Int: 0, Agi: 4, Vit: 0, Chk: 0, Luk: 0),
            ("Katana", ItemType.Weapon, ItemRarity.Rare, 500, "sword",
                Atk: 20, Def: 0, Int: 0, Agi: 5, Vit: 0, Chk: 0, Luk: 0),
            ("Espada Lendária — Kusanagi", ItemType.Weapon, ItemRarity.Legendary, 5000, "sword",
                Atk: 50, Def: 0, Int: 10, Agi: 15, Vit: 0, Chk: 20, Luk: 5),
            // Armaduras — foco em defesa e vitalidade
            ("Colete Chunin", ItemType.Armor, ItemRarity.Uncommon, 300, "shield",
                Atk: 0, Def: 15, Int: 0, Agi: 0, Vit: 10, Chk: 0, Luk: 0),
            ("Armadura Jounin", ItemType.Armor, ItemRarity.Rare, 1200, "shield",
                Atk: 0, Def: 30, Int: 5, Agi: 0, Vit: 20, Chk: 10, Luk: 0),
            ("Manto ANBU", ItemType.Armor, ItemRarity.Epic, 3000, "shield",
                Atk: 10, Def: 40, Int: 10, Agi: 10, Vit: 25, Chk: 15, Luk: 5),
            ("Armadura de Kage", ItemType.Armor, ItemRarity.Legendary, 10000, "shield",
                Atk: 15, Def: 60, Int: 15, Agi: 15, Vit: 40, Chk: 25, Luk: 10),
            // Acessórios — foco em inteligência e sorte
            ("Bandana Ninja", ItemType.Accessory, ItemRarity.Common, 100, "headphones",
                Atk: 0, Def: 0, Int: 3, Agi: 0, Vit: 0, Chk: 0, Luk: 2),
            ("Anel de Chakra", ItemType.Accessory, ItemRarity.Uncommon, 350, "circle",
                Atk: 0, Def: 0, Int: 5, Agi: 0, Vit: 0, Chk: 10, Luk: 0),
            ("Pingente da Sorte", ItemType.Accessory, ItemRarity.Rare, 800, "gem",
                Atk: 0, Def: 0, Int: 5, Agi: 0, Vit: 0, Chk: 5, Luk: 15),
            ("Olho do Dragão", ItemType.Accessory, ItemRarity.Epic, 2500, "eye",
                Atk: 5, Def: 0, Int: 20, Agi: 5, Vit: 0, Chk: 20, Luk: 10),
            // Ferramentas — bônus variados
            ("Papel Bomba", ItemType.Tool, ItemRarity.Uncommon, 120, "file-warning",
                Atk: 10, Def: 0, Int: 0, Agi: 0, Vit: 0, Chk: 0, Luk: 0),
            ("Pergaminho de Selamento", ItemType.Tool, ItemRarity.Rare, 400, "scroll",
                Atk: 0, Def: 5, Int: 15, Agi: 0, Vit: 0, Chk: 15, Luk: 0),
        };
        foreach (var (name, type, rarity, price, icon, atk, def, intel, agi, vit, chk, luk) in items)
        {
            var item = Item.Create(name, type, rarity, $"{GetRarityLabelPt(rarity)} — {name} ninja", price, icon);
            item.Equippable = type is ItemType.Weapon or ItemType.Armor or ItemType.Accessory or ItemType.Summon;
            item.Consumable = false;
            item.AttackBonus = atk;
            item.DefenseBonus = def;
            item.IntelligenceBonus = intel;
            item.AgilityBonus = agi;
            item.VitalityBonus = vit;
            item.ChakraBonus = chk;
            item.LuckBonus = luk;
            db.Items.Add(item);
        }

        // World Locations
        var locations = new[]
        {
            WorldLocation.Create("País do Fogo", LocationType.Pais, "Terra de Konoha, florestas densas e rios."),
            WorldLocation.Create("País do Vento", LocationType.Pais, "Terra de Suna, desertos e dunas."),
            WorldLocation.Create("País da Água", LocationType.Pais, "Terra de Kiri, arquipélagos e névoa."),
            WorldLocation.Create("Floresta da Morte", LocationType.Regiao, "Local do Exame Chunin, perigos mortais."),
            WorldLocation.Create("Vale do Fim", LocationType.Santuario, "Onde Hashirama e Madara lutaram."),
        };
        db.WorldLocations.AddRange(locations);

        // Achievements
        var achievements = new[]
        {
            Achievement.Create("Primeira Missão", "Complete sua primeira missão.", "scroll"),
            Achievement.Create("Shinobi Experiente", "Complete 50 missões.", "scroll-text"),
            Achievement.Create("Mestre dos Jutsus", "Aprenda 20 jutsus.", "sparkles"),
            Achievement.Create("Lendário", "Alcance nível 50.", "trophy"),
        };
        db.Achievements.AddRange(achievements);

        await db.SaveChangesAsync();
    }

    private static string GetRarityLabelPt(ItemRarity r) => r switch
    {
        ItemRarity.Common => "Comum",
        ItemRarity.Uncommon => "Incomum",
        ItemRarity.Rare => "Raro",
        ItemRarity.Epic => "Épico",
        ItemRarity.Legendary => "Lendário",
        _ => ""
    };
}
