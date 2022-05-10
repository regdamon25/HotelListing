using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelListing.API.Migrations
{
    public partial class AddedSizePropToHotelModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
     

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Hotels",
                type: "nvarchar(max)",
                nullable: true);

           
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Hotels");

            
        }
    }
}
