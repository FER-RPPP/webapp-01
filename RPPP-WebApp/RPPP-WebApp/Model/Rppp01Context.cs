using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RPPP_WebApp.Model;

public partial class Rppp01Context : DbContext
{

    public Rppp01Context(DbContextOptions<Rppp01Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Client { get; set; }

    public virtual DbSet<Document> Document { get; set; }

    public virtual DbSet<DocumentType> DocumentType { get; set; }

    public virtual DbSet<LaborDiary> LaborDiary { get; set; }

    public virtual DbSet<LaborType> LaborType { get; set; }

    public virtual DbSet<Organization> Organization { get; set; }

    public virtual DbSet<Owner> Owner { get; set; }

    public virtual DbSet<Project> Project { get; set; }

    public virtual DbSet<ProjectCard> ProjectCard { get; set; }

    public virtual DbSet<ProjectPartner> ProjectPartner { get; set; }

    public virtual DbSet<ProjectRequirement> ProjectRequirement { get; set; }

    public virtual DbSet<ProjectRole> ProjectRole { get; set; }

    public virtual DbSet<ProjectWork> ProjectWork { get; set; }

    public virtual DbSet<RequirementPriority> RequirementPriority { get; set; }

    public virtual DbSet<RequirementTask> RequirementTask { get; set; }

    public virtual DbSet<TaskStatus> TaskStatus { get; set; }

    public virtual DbSet<Transaction> Transaction { get; set; }

    public virtual DbSet<TransactionPurpose> TransactionPurpose { get; set; }

    public virtual DbSet<TransactionType> TransactionType { get; set; }

    public virtual DbSet<Worker> Worker { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("client");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("first_name");
            entity.Property(e => e.Iban)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("iban");
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("last_name");
            entity.Property(e => e.Oib)
                .IsRequired()
                .HasMaxLength(11)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("oib");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("document");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DocumentTypeId).HasColumnName("document_type_id");
            entity.Property(e => e.Format)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("format");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.Document)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_document_document_type");

            entity.HasOne(d => d.Project).WithMany(p => p.Document)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__document__projec__40F9A68C");
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.ToTable("document_type");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<LaborDiary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__labor_di__3213E83FB25D506B");

            entity.ToTable("labor_diary");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");
            entity.Property(e => e.HoursSpent)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("hours_spent");
            entity.Property(e => e.LaborDescription)
                .HasMaxLength(200)
                .HasColumnName("labor_description");
            entity.Property(e => e.LaborTypeId).HasColumnName("labor_type_id");
            entity.Property(e => e.WorkId).HasColumnName("work_id");
            entity.Property(e => e.WorkerId).HasColumnName("worker_id");

            entity.HasOne(d => d.LaborType).WithMany(p => p.LaborDiary)
                .HasForeignKey(d => d.LaborTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__labor_dia__labor__03F0984C");

            entity.HasOne(d => d.Work).WithMany(p => p.LaborDiary)
                .HasForeignKey(d => d.WorkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__labor_dia__work___02FC7413");

            entity.HasOne(d => d.Worker).WithMany(p => p.LaborDiary)
                .HasForeignKey(d => d.WorkerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__labor_dia__worke__40058253");
        });

        modelBuilder.Entity<LaborType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__labor_ty__3213E83F4C31883F");

            entity.ToTable("labor_type");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(25)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("organization_PK");

            entity.ToTable("organization");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.Oib).HasName("PK__owner__CB394B3FCC5E4292");

            entity.ToTable("owner");

            entity.Property(e => e.Oib)
                .HasMaxLength(11)
                .HasColumnName("OIB");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Surname)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("surname");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("project");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CardId)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("card_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.OwnerId)
                .IsRequired()
                .HasMaxLength(11)
                .HasColumnName("owner_id");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");

            entity.HasOne(d => d.Card).WithMany(p => p.Project)
                .HasForeignKey(d => d.CardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__project__card_id__3D2915A8");

            entity.HasOne(d => d.Client).WithMany(p => p.Project)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_project_client");

            entity.HasOne(d => d.Owner).WithMany(p => p.Project)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__project__owner_i__3B40CD36");
        });

        modelBuilder.Entity<ProjectCard>(entity =>
        {
            entity.HasKey(e => e.Iban).HasName("PK__project___8235CCBD8791C4CD");

            entity.ToTable("project_card");

            entity.Property(e => e.Iban)
                .HasMaxLength(50)
                .HasColumnName("IBAN");
            entity.Property(e => e.ActivationDate)
                .HasColumnType("date")
                .HasColumnName("activation_date");
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(20, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.Oib)
                .IsRequired()
                .HasMaxLength(11)
                .HasColumnName("OIB");

            entity.HasOne(d => d.OibNavigation).WithMany(p => p.ProjectCard)
                .HasForeignKey(d => d.Oib)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__project_car__OIB__2B0A656D");
        });

        modelBuilder.Entity<ProjectPartner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("project_partner_PK");

            entity.ToTable("project_partner");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DateFrom)
                .HasColumnType("date")
                .HasColumnName("date_from");
            entity.Property(e => e.DateTo)
                .HasColumnType("date")
                .HasColumnName("date_to");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.WorkerId).HasColumnName("worker_id");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectPartner)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_partner_FK");

            entity.HasOne(d => d.Role).WithMany(p => p.ProjectPartner)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_partner_FK_2");

            entity.HasOne(d => d.Worker).WithMany(p => p.ProjectPartner)
                .HasForeignKey(d => d.WorkerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_partner_FK_1");
        });

        modelBuilder.Entity<ProjectRequirement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__project___3213E83F87AA380A");

            entity.ToTable("project_requirement");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.RequirementPriorityId).HasColumnName("requirement_priority_id");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("type");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectRequirement)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__project_r__proje__37703C52");

            entity.HasOne(d => d.RequirementPriority).WithMany(p => p.ProjectRequirement)
                .HasForeignKey(d => d.RequirementPriorityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__project_r__requi__5DCAEF64");
        });

        modelBuilder.Entity<ProjectRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("project_role_PK");

            entity.ToTable("project_role");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ProjectWork>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__project___3213E83FB0DD6444");

            entity.ToTable("project_work");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AssigneeId).HasColumnName("assignee_id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("title");

            entity.HasOne(d => d.Assignee).WithMany(p => p.ProjectWork)
                .HasForeignKey(d => d.AssigneeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__project_w__assig__3F115E1A");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectWork)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__project_w__proje__7C4F7684");
        });

        modelBuilder.Entity<RequirementPriority>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__requirem__3213E83FCBEFA0B0");

            entity.ToTable("requirement_priority");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");
        });

        modelBuilder.Entity<RequirementTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__requirem__3213E83F78FC50F1");

            entity.ToTable("requirement_task");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActualEndDate)
                .HasColumnType("date")
                .HasColumnName("actual_end_date");
            entity.Property(e => e.ActualStartDate)
                .HasColumnType("date")
                .HasColumnName("actual_start_date");
            entity.Property(e => e.PlannedEndDate)
                .HasColumnType("date")
                .HasColumnName("planned_end_date");
            entity.Property(e => e.PlannedStartDate)
                .HasColumnType("date")
                .HasColumnName("planned_start_date");
            entity.Property(e => e.ProjectRequirementId).HasColumnName("project_requirement_id");
            entity.Property(e => e.TaskStatusId).HasColumnName("task_status_id");

            entity.HasOne(d => d.ProjectWork).WithOne(p => p.RequirementTask)
                .HasForeignKey<RequirementTask>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__requirement___id__4F47C5E3");

            entity.HasOne(d => d.ProjectRequirement).WithMany(p => p.RequirementTask)
                .HasForeignKey(d => d.ProjectRequirementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__requireme__proje__367C1819");

            entity.HasOne(d => d.TaskStatus).WithMany(p => p.RequirementTask)
                .HasForeignKey(d => d.TaskStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__requireme__task___6383C8BA");
        });

        modelBuilder.Entity<TaskStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__task_sta__3213E83FC6464186");

            entity.ToTable("task_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transact__3213E83F05FC0CB8");

            entity.ToTable("transaction");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(20, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Iban)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("IBAN");
            entity.Property(e => e.PurposeId).HasColumnName("purpose_id");
            entity.Property(e => e.Recipient)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("recipient");
            entity.Property(e => e.TypeId).HasColumnName("type_id");

            entity.HasOne(d => d.IbanNavigation).WithMany(p => p.Transaction)
                .HasForeignKey(d => d.Iban)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__transactio__IBAN__4A8310C6");

            entity.HasOne(d => d.Purpose).WithMany(p => p.Transaction)
                .HasForeignKey(d => d.PurposeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__transacti__purpo__4C6B5938");

            entity.HasOne(d => d.Type).WithMany(p => p.Transaction)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__transacti__type___4B7734FF");
        });

        modelBuilder.Entity<TransactionPurpose>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transact__3213E83F949918FD");

            entity.ToTable("transaction_purpose");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.PurposeName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("purpose_name");
        });

        modelBuilder.Entity<TransactionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transact__3213E83FA566368B");

            entity.ToTable("transaction_type");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.TypeName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("worker_PK");

            entity.ToTable("worker");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("last_name");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone_number");

            entity.HasOne(d => d.Organization).WithMany(p => p.Worker)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("worker_FK");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
