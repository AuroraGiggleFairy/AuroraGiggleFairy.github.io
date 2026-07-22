
blocks.xml:
<block name="forge">
	<!-- Class -->
	<property name="Class" value="Forge" />
	<property class="Workstation">
		<property name="CraftingAreaRecipes" value="forge" />
		<property name="Modules" value="tools,output,fuel,material_input" />
		<property name="InputMaterials" value="iron,brass,lead,glass,stone,clay" />
		<property name="ToolNames" value="1,2,3" />
	</property>
	<property name="UnlockedBy" value="craftingWorkstations" />
	<!-- UI_Info -->
	<property name="CreativeMode" value="Player" />
	<property name="DescriptionKey" value="forgeDesc" />
	<property name="WorkstationIcon" value="ui_game_symbol_forge" />
	<property name="Stacknumber" value="1" />
	<!-- Visual -->
	<property name="Material" value="Mstone_scrap" />
	<property name="Shape" value="ModelEntity" />
	<property name="Model" value="@:Entities/Crafting/forgeWorkstationPrefab.prefab" />
	<property name="MultiBlockDim" value="2,2,1" />
	<property name="ImposterDontBlock" value="true" />
	<property name="WaterFlow" value="permitted" />
	<property name="ParticleName" value="forgeWorkstation" />
	<property name="ParticleOffset" value="0,0.4,0.5" />
	<!-- Placement -->
	<property name="Place" value="TowardsPlacerInverted" />
	<property name="OnlySimpleRotations" value="true" />
	<property name="Path" value="solid" />
	<property name="IsTerrainDecoration" value="true" />
	<property name="StabilitySupport" value="true" />
	<!-- Value -->
	<property name="Weight" value="0" />
	<property name="EconomicValue" value="1000" />
	<property name="TraderStageTemplate" value="midTier0" /><!-- forge -->
	<!-- Interaction -->
	<property name="MaxDamage" value="800" />
	<property name="HeatMapStrength" value="6" />
	<property name="HeatMapTime" value="5000" />
	<property name="HeatMapFrequency" value="1000" />
	<property name="BuffsWhenWalkedOn" value="buffBurningEnvironment" />
	<property name="ActiveRadiusEffects" value="buffCampfireAOE,5" />
	<property name="OpenSound" value="forge_open" />
	<property name="CloseSound" value="forge_close" />
	<property name="CraftSound" value="forge_smelt_click" />
	<property name="CraftCompleteSound" value="forge_item_complete" />
	<property name="TakeDelay" value="15" />
	<property class="RepairItems">
		<property name="resourceCobblestones" value="50" />
	</property>
	<drop event="Harvest" name="resourceRockSmall" count="40" tag="allHarvest,perkJunkMiner" />
	<drop event="Harvest" name="resourceClayLump" count="20" tag="allHarvest,perkJunkMiner" />
	<drop event="Harvest" name="resourceLeather" count="5" tag="allHarvest,perkJunkMiner" />
	<drop event="Destroy" count="0" />
	<drop event="Fall" name="terrDestroyedStone" count="1" prob="0.75" stick_chance="1" />
	<!-- Sorting -->
	<property name="SortOrder1" value="B281" />
	<property name="SortOrder2" value="0200" />
	<property name="Group" value="Building,TCScience,advBuilding" />
	<property name="Tags" value="workstationSkill,twitch_workstation" />
	<property name="FilterTags" value="MC_playerBlocks,SC_decor" />
	<property name="SoundPickup" value="forge_grab" />
	<property name="SoundPlace" value="forge_place" />
</block>


xui.xml:
	<window_group name="workstation_forge" controller="XUiC_WorkstationWindowGroup" open_backpack_on_open="true" close_compass_on_open="true" defaultselected="bp.content">
		<window name="windowCraftingList"/>
		<window name="craftingInfoPanel"/>
		<window name="windowCraftingQueue"/>
		<window name="windowToolsForge" />
		<window name="windowFuel" />
		<window name="windowForgeInput" />
		<window name="windowOutput" />
		<window name="windowNonPagingHeader" />
	</window_group>


windows.xml:
	<window name="windowCraftingQueue" width="397" height="78" panel="Left" cursor_area="true" always_update="true">
		<rect depth="0" pos="96,0" width="303" controller="CraftingQueue" always_update="true">
			<!-- <sprite name="background" color="[black]" type="sliced" pos="0,-10" /> -->
			<grid name="queue" rows="1" cols="4" pos="3,-13" cell_width="75" cell_height="75" repeat_content="true" always_update="true">
				<recipe_stack name="0"/>
			</grid>
		</rect>
	</window>

	<window name="craftingInfoPanel" width="603" height="392" controller="CraftingInfoWindow" style="crafting.info.window" panel="Center" cursor_area="true" >
		<rect name="header" height="43" depth="1">
			<headerbg />
			<sprite depth="2" name="windowIcon" style="icon32px" pos="4,-5" sprite="{itemgroupicon}"/>
			<label style="header.name" text="{itemname}" />

			<rect pos="350,0" name="requiredToolOverlay">
				<sprite size="24,24" depth="2" name="requiredToolCheckmark" pos="0,-8" sprite="ui_game_symbol_check" color="[red]"/>
				<label depth="2" pos="35,-8" name="requiredToolText" width="200" height="32" text="Required" text_key="xuiRequired" font_size="32" upper_case="true" justify="left"/>
			</rect>
		</rect>

		<rect name="contentCraftingInfo" height="381" depth="1" pos="0,-46">
			<sprite depth="5" name="backgroundMain" sprite="menu_empty3px" width="604" height="348" color="[black]" type="sliced" fillcenter="false" />
			<rect depth="1" pos="3,-3" name="preview" width="147" height="169">
				<sprite depth="8" name="backgroundMain" sprite="menu_empty3px" pos="-3,3" width="153" height="175" color="[black]" type="sliced" fillcenter="false" />
				<sprite depth="1" color="[darkGrey]" type="sliced" />
				<sprite depth="12" name="itemPreview" width="110" height="110" atlas="ItemIconAtlas" sprite="{itemicon}" color="{itemicontint}" pos="74,-58" pivot="center" foregroundlayer="true"/>
				<sprite depth="8" name="itemtypeicon" width="32" height="32" sprite="ui_game_symbol_{itemtypeicon}" pos="2,-2" foregroundlayer="true" visible="{hasitemtypeicon}" color="{itemtypeicontint}" />
				<sprite depth="3" name="durabilityBackground" height="20" width="85" color="48,48,48,255" type="sliced" pos="31, -113" visible="{hasdurability}"/>
				<sprite depth="4" name="durabilityBar" height="20" width="85" color="{durabilitycolor}" type="filled" fillspritepad="true" pos="31, -113" fill="{durabilityfill}" visible="{hasdurability}" />
				<label depth="12" name="durabilityValue" pos="0,-104" width="145" height="32" text="{durabilitytext}" font_size="30" justify="{durabilityjustify}" effect="outline" />

				<sprite depth="3" name="clockIcon" size="24,24" sprite="ui_game_symbol_clock" pos="25, -142" type="sliced" color="[iconColor]" foregroundlayer="true" />
				<!-- <label depth="3" name="TimeLabel" style="icon30px" pos="53, -119" text="TIME" text_key="xuiTime" font_size="22" /> -->
				<label depth="3" name="craftingTime" width="100" height="32" pos="55, -143" text="{craftingtime}" font_size="26" color="[beige]"/>
				<button depth="12" name="addQualityButton" style="icon22px, press" pos="132,-123" sprite="ui_game_symbol_arrow_menu" flip="Horizontally" pivot="center" sound="[paging_click]" visible="{hasdurability}" disabledcolor="[lightGrey]" enabled="{enableaddquality}" />
				<button depth="12" name="subtractQualityButton" style="icon22px, press" pos="14,-123" sprite="ui_game_symbol_arrow_menu" pivot="center" sound="[paging_click]" visible="{hasdurability}" disabledcolor="[lightGrey]" enabled="{enablesubtractquality}" />
			</rect>

			<sprite depth="8" name="backgroundMain" sprite="menu_empty3px" pos="0,-174" width="153" height="173" color="[black]" type="sliced" fillcenter="false" />
			<grid name="itemActions" rows="4" cols="1" pos="3,-176" width="148" cell_width="147" cell_height="42" controller="ItemActionList">
				<rect depth="1" name="actions" width="147" height="225">
					<sprite color="[mediumGrey]" type="sliced" height="45" />
					<rect name="recipeCraftCountControl" width="120" height="210" pos="27,0" controller="RecipeCraftCount">
						<button depth="2" name="countDown" style="icon30px, press, held" pos="-10,-20" sprite="ui_game_symbol_arrow_left" pivot="center" sound="[paging_click]" disabledcolor="[lightGrey]" enabled="{enablecountdown}"/>
						<textfield name="count_input" depth="2" pos="7,-6" width="40" height="28" character_limit="4" validation="integer" virtual_keyboard_prompt="vkPromptCount" />
						<button depth="2" name="countUp" style="icon30px, press, held" pos="64,-20" sprite="ui_game_symbol_arrow_right" pivot="center" sound="[paging_click]" disabledcolor="[lightGrey]" enabled="{enablecountup}"/>
						<button depth="2" name="countMax" style="icon30px, press" pos="96,-20" sprite="ui_game_symbol_arrow_max" pivot="center" sound="[paging_click]" disabledcolor="[lightGrey]" enabled="{enablecountup}"/>
					</rect>
				</rect>
				<item_action_entry />
				<item_action_entry />
				<item_action_entry />
				<!-- <sprite depth="3" name="fillerBackground" height="22" color="[mediumGrey]" type="sliced"/> -->
			</grid>

			<rect depth="2" name="searchControls" width="449" height="43" pos="152,0">
				<sprite depth="1" color="[mediumGrey]" type="sliced" />
				<button depth="4" name="ingredientsButton" style="icon30px, press" pos="22,-22" sprite="ui_game_symbol_resource" pivot="center" tooltip_key="ingredient" sound="[paging_click]" selected="true" />
				<button depth="4" name="descriptionButton" style="icon30px, press" pos="65,-22" sprite="ui_game_symbol_book" pivot="center" tooltip_key="lblBookDesc" sound="[paging_click]" />
				<button depth="4" name="showunlocksButton" style="icon30px, press" pos="108,-22" sprite="ui_game_symbol_unlock" pivot="center" tooltip_key="xuiSkillUnlocks" sound="[paging_click]" visible="{showunlockedbytab}" />
			</rect>


			<rect depth="1" pos="153,-43" name="description" width="447" height="328" visible="{showdescription}">

				<sprite depth="3" name="backgroundMain" sprite="menu_empty3px" pos="-3,0" width="453" height="303" color="[black]" type="sliced" fillcenter="false" />

				<rect>
					<sprite depth="1" color="[darkGrey]" type="sliced" height="301" />
					<label depth="3" name="descriptionText" pos="6,-5" text="{itemdescription}"  width="440" height="294" parse_actions="true" />
				</rect>

			</rect>

			<rect depth="1" pos="153,-45" name="ingredients" width="447" height="264" visible="{showingredients}">
				<grid rows="6" width="447" height="231" cell_height="50" cell_width="447" controller="IngredientList" arrangement="vertical">
					<ingredient_header name="0"/>
					<ingredient_row name="1"/>
					<ingredient_row name="2"/>
					<ingredient_row name="3"/>
					<ingredient_row name="4"/>
					<ingredient_row name="5"/>
				</grid>
			</rect>

			<rect depth="1" pos="153,-45" name="unlockedBy" width="447" height="264" visible="{showunlockedby}">
				<grid rows="6" width="447" height="231" cell_height="50" cell_width="447" controller="UnlockByList" arrangement="vertical">
					<unlocked_by_header name="0"/>
					<unlocked_by_row name="1"/>
					<unlocked_by_row name="2"/>
					<unlocked_by_row name="3"/>
					<unlocked_by_row name="4"/>
					<unlocked_by_row name="5"/>
				</grid>
			</rect>
		</rect>
	</window>

	<window name="windowCraftingList"  width="397" height="688" controller="CraftingListInfo" panel="Left" cursor_area="true" >

		<rect name="header" height="43" depth="1">
			<headerbg />
			<sprite pos="4,-5" depth="2" name="windowIcon" style="icon32px" sprite="Craft_Icon_Basics"/>
			<label style="header.name" text="basics" text_key="xuiBasics" />

			<!-- <label pos="387, -6" depth="2" name="unlockedCount" width="64" height="32" text="0/65" font_size="32" color="[lightGrey]" justify="right" pivot="topright"/> -->
			<!-- <sprite pos="328, -5" depth="2" name="unlockedIcon" style="icon32px" sprite="ui_game_symbol_book" color="[lightGrey]" pivot="topright"/> -->
		</rect>

		<rect name="content" height="650" depth="1" pos="0,-43" on_scroll="true">

			<rect depth="2" name="categorySelector" width="390" height="44" pos="3,-6">
				<sprite name="backgroundMain" sprite="menu_empty3px" pos="-3,3" width="396" height="49" color="[black]" type="sliced" fillcenter="false" />
				<sprite color="[mediumGrey]" type="sliced" />
				<grid name="categories" pos="23,-21" rows="1" cols="9" width="390" height="43" cell_width="43" cell_height="43" repeat_content="true" controller="CategoryList">
					<category_icon />
				</grid>
			</rect>

			<rect depth="3" name="searchControls" width="390" height="44" pos="3,-52">
				<sprite name="backgroundMain" sprite="menu_empty3px" pos="-3,3" width="396" height="49" color="[black]" type="sliced" fillcenter="false" />
				<sprite color="[darkGrey]" type="sliced" />
				<button depth="4" name="favorites" style="icon30px, press" pos="18,-22" sprite="server_favorite" pivot="center" sound="[paging_click]" tooltip="Favorites" tooltip_key="lblFavorites" collider_scale="1.5" />

				<rect pos="104,0" width="200">
					<sprite depth="4" name="searchIcon" style="icon30px" pos="0,-22" sprite="ui_game_symbol_search" pivot="center"/>
					<textfield depth="5" name="searchInput" pos="22,-7" width="140" height="30" virtual_keyboard_prompt="vkPromptSearchTerm" search_field="true" close_group_on_tab="true" clear_button="true" />
				</rect>

				<rect pos="286,0" width="104" height="43">
					<pager name="pager" pos="4,-6" contents_parent="content"/>
				</rect>
			</rect>

			<grid name="recipes" depth="2" rows="12" cols="1" pos="3,-98" width="390" height="552" cell_width="390" cell_height="46" controller="RecipeList" repeat_content="true" arrangement="vertical" >
				<recipe_entry name="0"/>
			</grid>
		</rect>
	</window>

	<window name="windowToolsForge" width="228" height="121" panel="Right" cursor_area="true" >
		<rect style="header.panel">
			<headerbg />
			<sprite style="header.icon" sprite="ui_game_symbol_wrench"/>
			<label style="header.name.shrink" text="TOOLS" text_key="xuiTools" />
		</rect>

		<rect name="content" depth="0" pos="0,-46" height="75" disablefallthrough="true">

			<grid name="inventory" rows="1" cols="3" pos="3,-3" cell_width="75" cell_height="75" controller="WorkstationToolGrid" repeat_content="true"
			required_tools="toolBellows,toolAnvil,toolForgeCrucible" required_tools_only="true">
				<item_stack controller="RequiredItemStack" name="0"/>
			</grid>
		</rect>

	</window>
 	<window name="windowFuel" width="228" height="166" panel="Right" cursor_area="true">
		<rect style="header.panel">
			<headerbg />
			<sprite style="header.icon" sprite="ui_game_symbol_fire"/>
			<label style="header.name.iconright" text="FUEL" text_key="xuiFuel" />
			<label style="header.timer"/>
		</rect>

		<rect name="content" depth="0" pos="0,-46" height="79" >
			<rect disablefallthrough="true">
				<grid rows="1" cols="3" pos="3,-3" cell_width="75" cell_height="75" controller="WorkstationFuelGrid" repeat_content="true">
					<item_stack name="0"/>
				</grid>
			</rect>
			<grid name="slot_preview" depth="1" rows="1" cols="3" pos="3,-3" cell_width="75" cell_height="75" controller="SlotPreview">
				<rect>
					<sprite name="slot" depth="2" width="64" height="64" sprite="resourceWood" atlas="ItemIconAtlasGreyscale" pos="35,-35" pivot="center" foregroundlayer="true"/>
				</rect>
				<rect>
					<sprite name="slot" depth="2" width="64" height="64" sprite="resourceWood" atlas="ItemIconAtlasGreyscale" pos="35,-35" pivot="center" foregroundlayer="true"/>
				</rect>
				<rect>
					<sprite name="slot" depth="2" width="64" height="64" sprite="resourceWood" atlas="ItemIconAtlasGreyscale" pos="35,-35" pivot="center" foregroundlayer="true"/>
				</rect>
			</grid>
		</rect>

		<rect name="buttonContent" depth="5" pos="0, -121" height="40">
			<sprite depth="5" name="backgroundMain" sprite="menu_empty3px" color="[black]" type="sliced" fillcenter="false" />
			<rect depth="1" pos="3,-3" width="225" height="34">
				<button name="button" sprite="menu_empty" defaultcolor="[mediumGrey]" disabledcolor="[mediumGrey]" hoversprite="ui_game_select_row" hovercolor="[white]" type="sliced" width="222"  hoverscale="1.0" />
				<sprite depth="2" name="flameIcon" style="icon32px" pos="5,0" sprite="ui_game_symbol_fire" />
				<label depth="2" name="onoff" pos="0,-6" justify="center" text="TURN ON" text_key="xuiTurnOn" font_size="26" />
			</rect>
		</rect>
	</window>
   	<window name="windowForgeInput" width="228" height="204" panel="Right"
		controller="WorkstationMaterialInputWindow" materials_accepted="iron,brass,lead,glass,stone,clay" valid_materials_color="[green]" invalid_materials_color="[red]" cursor_area="true" >
		<rect style="header.panel">
			<headerbg />
			<sprite style="header.icon" sprite="ui_game_symbol_forge"/>
			<label style="header.name.shrink" text="INPUT" text_key="xuiSmelting" />
		</rect>

		<sprite depth="2" name="backgroundMain" sprite="menu_empty3px" pos="0,-46" height="153" color="[black]" type="sliced" fillcenter="false" />
		<rect name="content" depth="1" pos="0,-46" height="153">

			<grid depth="7" rows="2" cols="1" pos="3,-3" cell_width="75" cell_height="75" controller="WorkstationMaterialInputGrid" repeat_content="true">
				<item_stack name="0"/>
			</grid>

		</rect>

		<rect name="content2" depth="0" pos="78, -49" width="147" height="148">
			<sprite depth="1" color="[mediumGrey]" type="sliced"/>
			<grid rows="6" cols="1" pos="3,-3" cell_width="147" cell_height="24"  repeat_content="true">
				<forge_material name="0"/>
			</grid>
		</rect>
	</window>    
    	<window name="windowOutput" width="228" height="198" anchor="Top" panel="Right" cursor_area="true" controller="WorkstationOutputWindow" >
		<rect style="header.panel">
			<headerbg />
			<sprite style="header.icon" sprite="ui_game_symbol_loot_sack"/>
			<label style="header.name.iconright" text="OUTPUT" text_key="xuiOutput" />
			
			<rect pos="0,0" controller="ContainerStandardControls" visible="{buttons_visible}">
				<button   depth="3" name="btnMoveAll"          sprite="ui_game_symbol_store_all_up"     tooltip="{take_all_tooltip}"          pos="203, -22" style="icon32px, press, hover" pivot="center" sound="[paging_click]" />
			</rect>
		</rect>


		<rect name="content" depth="0" pos="0,-46" height="150" disablefallthrough="true">
			<sprite depth="2" name="backgroundMain" sprite="menu_empty3px" height="150" color="[black]" type="sliced" fillcenter="false" />
				<grid depth="10" name="inventory" rows="2" cols="3" pos="3,-3" cell_width="75" cell_height="75" controller="WorkstationOutputGrid" repeat_content="true">
					<item_stack name="0"/>
				</grid>
		</rect>
	</window>


