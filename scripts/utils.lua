function table_contains(table, element)
	for _, value in pairs(table) do
		if value == element then
			return true
		end
	end
	return false
end

function apply_drag(velocity, drag, dt)
	-- Scale the velocity by the drag factor (only horizontal)
	local drag_factor = math.pow(drag, dt)
	velocity.x = velocity.x * drag_factor

	-- Ensure that the velocity does not reverse direction due to floating point errors
	if math.abs(velocity.x) < 0.001 then
		velocity.x = 0
	end

	return velocity
end

function clamp(val, min, max)
	return math.max(math.min(val, max), min)
end