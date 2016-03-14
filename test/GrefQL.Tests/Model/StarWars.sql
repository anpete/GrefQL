if not exists(select * from sys.databases where name = 'GrephQL.StarWars')
    create database [GrephQL.StarWars];

if exists (select * from sysobjects where name='Humans' and xtype='U')
    drop table Humans;

create table Humans
(
    Id nvarchar(50) not null primary key,
    Name nvarchar(50),
    HomePlanet nvarchar(50)
);

insert into Humans values ('1', 'Luke', 'Tatooine');
insert into Humans values ('2', 'Vader', 'Tatooine');