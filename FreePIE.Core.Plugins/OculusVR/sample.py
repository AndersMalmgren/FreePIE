diagnostics.watch(oculusVR.isHmdMounted)
diagnostics.watch(oculusVR.buttons)
diagnostics.watch(oculusVR.touches)

diagnostics.watch(oculusVR.headStatus)
diagnostics.watch(oculusVR.leftHandStatus)
diagnostics.watch(oculusVR.rightHandStatus)

diagnostics.watch(oculusVR.headIsTracking)
diagnostics.watch(oculusVR.leftHandIsTracking)
diagnostics.watch(oculusVR.rightHandIsTracking)

diagnostics.watch(oculusVR.head.yawDegrees)
diagnostics.watch(oculusVR.head.pitchDegrees)
diagnostics.watch(oculusVR.head.rollDegrees)

if oculusVR.headIsTracking:
	diagnostics.watch(oculusVR.head.x)
	diagnostics.watch(oculusVR.head.y)
	diagnostics.watch(oculusVR.head.z)

diagnostics.watch(oculusVR.leftHand.yawDegrees)
diagnostics.watch(oculusVR.leftHand.pitchDegrees)
diagnostics.watch(oculusVR.leftHand.rollDegrees)
if oculusVR.leftHandIsTracking:
	diagnostics.watch(oculusVR.leftHand.x)
	diagnostics.watch(oculusVR.leftHand.y)
	diagnostics.watch(oculusVR.leftHand.z)

diagnostics.watch(oculusVR.rightHand.yawDegrees)
diagnostics.watch(oculusVR.rightHand.pitchDegrees)
diagnostics.watch(oculusVR.rightHand.rollDegrees)
if oculusVR.rightHandIsTracking:
	diagnostics.watch(oculusVR.rightHand.x)
	diagnostics.watch(oculusVR.rightHand.y)
	diagnostics.watch(oculusVR.rightHand.z)