﻿// <auto-generated />
using System;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NftIndexer.Entities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NftIndexer.Migrations
{
    [DbContext(typeof(NftIndexerContext))]
    [Migration("20230722103901_TokensContractRelation")]
    partial class TokensContractRelation
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("NftIndexer.Entities.Contract", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("address");

                    b.Property<string>("ContractType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("contract_type");

                    b.Property<long>("LastBlockEvent")
                        .HasColumnType("bigint")
                        .HasColumnName("last_block_event");

                    b.Property<long>("LastBlockSynced")
                        .HasColumnType("bigint")
                        .HasColumnName("last_block_synced");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Symbol")
                        .HasColumnType("text")
                        .HasColumnName("symbol");

                    b.Property<string>("Uri")
                        .HasColumnType("text")
                        .HasColumnName("uri");

                    b.HasKey("Id")
                        .HasName("pk_contracts");

                    b.ToTable("contracts", (string)null);
                });

            modelBuilder.Entity("NftIndexer.Entities.SyncInfo", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Error")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("error");

                    b.Property<long>("FromBlock")
                        .HasColumnType("bigint")
                        .HasColumnName("from_block");

                    b.Property<DateTime>("Time")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("time");

                    b.Property<long>("ToBlock")
                        .HasColumnType("bigint")
                        .HasColumnName("to_block");

                    b.HasKey("Id")
                        .HasName("pk_sync_infos");

                    b.ToTable("sync_infos", (string)null);
                });

            modelBuilder.Entity("NftIndexer.Entities.Token", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("ContractId")
                        .HasColumnType("bigint")
                        .HasColumnName("contract_id");

                    b.Property<string>("Metadatas")
                        .HasColumnType("text")
                        .HasColumnName("metadatas");

                    b.Property<BigInteger>("TokenId")
                        .HasColumnType("numeric")
                        .HasColumnName("token_id");

                    b.Property<string>("Uri")
                        .HasColumnType("text")
                        .HasColumnName("uri");

                    b.HasKey("Id")
                        .HasName("pk_tokens");

                    b.HasIndex("ContractId")
                        .HasDatabaseName("ix_tokens_contract_id");

                    b.ToTable("tokens", (string)null);
                });

            modelBuilder.Entity("NftIndexer.Entities.TokenHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("Amount")
                        .HasColumnType("bigint")
                        .HasColumnName("amount");

                    b.Property<long>("BlockHash")
                        .HasColumnType("bigint")
                        .HasColumnName("block_hash");

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint")
                        .HasColumnName("block_number");

                    b.Property<string>("Error")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("error");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("event_type");

                    b.Property<string>("From")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("from");

                    b.Property<long>("LogIndex")
                        .HasColumnType("bigint")
                        .HasColumnName("log_index");

                    b.Property<string>("Metadatas")
                        .HasColumnType("text")
                        .HasColumnName("metadatas");

                    b.Property<DateTime>("Time")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("time");

                    b.Property<string>("To")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("to");

                    b.Property<long>("TokenId")
                        .HasColumnType("bigint")
                        .HasColumnName("token_id");

                    b.Property<string>("TransactionHash")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("transaction_hash");

                    b.Property<long>("TransactionIndex")
                        .HasColumnType("bigint")
                        .HasColumnName("transaction_index");

                    b.Property<string>("Uri")
                        .HasColumnType("text")
                        .HasColumnName("uri");

                    b.HasKey("Id")
                        .HasName("pk_token_histories");

                    b.HasIndex("TokenId")
                        .HasDatabaseName("ix_token_histories_token_id");

                    b.ToTable("token_histories", (string)null);
                });

            modelBuilder.Entity("NftIndexer.Entities.Token", b =>
                {
                    b.HasOne("NftIndexer.Entities.Contract", "Contract")
                        .WithMany("Tokens")
                        .HasForeignKey("ContractId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_tokens_contracts_contract_id");

                    b.Navigation("Contract");
                });

            modelBuilder.Entity("NftIndexer.Entities.TokenHistory", b =>
                {
                    b.HasOne("NftIndexer.Entities.Token", "Token")
                        .WithMany("TokenHistories")
                        .HasForeignKey("TokenId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_token_histories_tokens_token_id");

                    b.Navigation("Token");
                });

            modelBuilder.Entity("NftIndexer.Entities.Contract", b =>
                {
                    b.Navigation("Tokens");
                });

            modelBuilder.Entity("NftIndexer.Entities.Token", b =>
                {
                    b.Navigation("TokenHistories");
                });
#pragma warning restore 612, 618
        }
    }
}
