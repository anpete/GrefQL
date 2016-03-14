if not exists(select * from sys.databases where name = 'GrephQL.StarWars')
    create database [GrephQL.StarWars];

if exists (select * from sysobjects where name='Character' and xtype='U')
    drop table [Character];

create table [Character]
(
    Id nvarchar(50) not null primary key,
    Name nvarchar(50),
    HomePlanet nvarchar(50),
	PrimaryFunction nvarchar(50),
	Discriminator nvarchar(50)
);

insert into [Character] values ('1', 'Luke', 'Tatooine', null, 'Human');
insert into [Character] values ('2', 'Vader', 'Tatooine', null, 'Human');
insert into [Character] values ('3', 'R2-D2', null, 'Astromech', 'Droid');
insert into [Character] values ('4', 'Vader', null, 'Protocol', 'Droid');